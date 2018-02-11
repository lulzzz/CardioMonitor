using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.CheckPoints;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.ForcedDataCollectionRequest;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Monitor.Infrastructure;
using JetBrains.Annotations;
using Polly;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.PressureParams
{
    internal class PumpingManager : ICycleProcessingPipelineElement
    {
        /// <summary>
        /// Таймаут операции накачки
        /// </summary>
        private readonly TimeSpan _pumpingTimeout;
        
        [NotNull]
        private readonly IMonitorController _monitorController;

        public PumpingManager([NotNull] IMonitorController monitorController)
        {
            _monitorController = monitorController ?? throw new ArgumentNullException(nameof(monitorController));
            
            _pumpingTimeout = new TimeSpan(0, 0, 8);
        }

        public async Task<CycleProcessingContext> ProcessAsync(CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var needPumping = context.TryGetAutoPumpingRequestParams()?.IsAutoPumpingEnabled ?? false;

            if (!needPumping)
            {
                context.AddOrUpdate(new PumpingResultContextParams(true));
                return context;
            }

            var retryCounts = context.TryGetAutoPumpingRequestParams()?.PumpingNumberOfAttempts ?? 0;

            bool wasPumpingComleted;
            try
            {
                var timeoutPolicy = Policy
                    .TimeoutAsync(_pumpingTimeout);
                var recilencePolicy = Policy
                    .Handle<Exception>()
                    .RetryAsync(retryCounts);
                var policyWrap = Policy.WrapAsync(timeoutPolicy, recilencePolicy);
                await policyWrap
                    .ExecuteAsync(_monitorController.PumpCuffAsync)
                    .ConfigureAwait(false);
                wasPumpingComleted = true;
            }
            catch (DeviceConnectionException e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.MonitorConnectionError,
                            e.Message,
                            e)));
                wasPumpingComleted = false;
            }
            catch (TimeoutException e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.PumpingTimeout,
                            e.Message,
                            e)));
                wasPumpingComleted = false;
            }
            catch (Exception e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.PumpingError,
                            e.Message,
                            e)));
                wasPumpingComleted = false;
            }

            context.AddOrUpdate(new PumpingResultContextParams(wasPumpingComleted));

            return context;
        }

        public bool CanProcess(CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var forcedRequest = context.TryGetForcedDataCollectionRequest();
            if (forcedRequest != null && forcedRequest.IsRequested)
            {
                return true;
            }
            
            var checkPointReachedParams = context.TryGetCheckPointParams();
            return checkPointReachedParams != null && checkPointReachedParams.NeedRequestEcg;
        }
    }
}