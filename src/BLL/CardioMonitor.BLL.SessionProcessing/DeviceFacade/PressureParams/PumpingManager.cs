using System;
using System.Threading;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.CheckPoints;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.ForcedDataCollectionRequest;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Monitor.Infrastructure;
using JetBrains.Annotations;
using Markeli.Utils.Logging;
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

        private ILogger _logger;
        private readonly object _lockObject;

        public PumpingManager([NotNull] IMonitorController monitorController,
            TimeSpan pumpingTimeout)
        {
            _monitorController = monitorController ?? throw new ArgumentNullException(nameof(monitorController));
            
            _pumpingTimeout = pumpingTimeout;
            _lockObject = new object();
        }

        public async Task<CycleProcessingContext> ProcessAsync(CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var needPumping = context.TryGetAutoPumpingRequestParams()?.IsAutoPumpingEnabled ?? false;

            _logger?.Trace($"{GetType().Name}: накачка манжеты...");
            if (!needPumping)
            {
                _logger?.Trace($"{GetType().Name}: накачка манжеты не требуется");
                context.AddOrUpdate(new PumpingResultContextParams(true));
                return context;
            }

            var retryCounts = context.TryGetAutoPumpingRequestParams()?.PumpingNumberOfAttempts ?? 0;

            bool wasPumpingComleted;
            try
            {
                if (!Monitor.TryEnter(_lockObject))
                {
                    _logger?.Warning($"{GetType().Name}: предыдущий запрос еще выполняется. Новый запрос не будет выполнен");
                    return context;
                }

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
            finally
            {
                if (Monitor.IsEntered(_lockObject))
                {
                    Monitor.Exit(_lockObject);
                }
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

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }
    }
}