using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Angle;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.CheckPoints;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.ForcedDataCollectionRequest;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Iterations;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Time;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Monitor.Infrastructure;
using JetBrains.Annotations;
using Markeli.Utils.Logging;
using Polly;
using Polly.Timeout;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.PressureParams
{
    internal class PatientPressureParamsProvider : ICycleProcessingPipelineElement
    {

        /// <summary>
        /// Таймаут запроса параметром пациента
        /// </summary>
        private readonly TimeSpan _updatePatientParamTimeout;
        [NotNull]
        private readonly IMonitorController _monitorController;

        private ILogger _logger;

        public PatientPressureParamsProvider(
            [NotNull] IMonitorController monitorController,
            TimeSpan operationsTimeout)
        {
            _monitorController = monitorController ?? throw new ArgumentNullException(nameof(monitorController));
            
            _updatePatientParamTimeout = operationsTimeout;
            
        }

        public async Task<CycleProcessingContext> ProcessAsync([NotNull] CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var pumpingResult = context.TryGetAutoPumpingResultParams();
            if (!(pumpingResult?.WasPumpingCompleted ?? false)) return context;

            Devices.Monitor.Infrastructure.PatientPressureParams param = null;
            
            var sessionInfo = context.TryGetSessionProcessingInfo();
            var cycleNumber = sessionInfo?.CurrentCycleNumber;
            var iterationInfo = context.TryGetIterationParams();
            var iterationNumber = iterationInfo?.CurrentIteration;
           
            try
            {
                _logger?.Trace($"{GetType().Name}: запрос показателей давления");
                var timeoutPolicy = Policy.TimeoutAsync(_updatePatientParamTimeout);
                param = await timeoutPolicy.ExecuteAsync(
                        _monitorController
                            .GetPatientPressureParamsAsync)
                    .ConfigureAwait(false);
            }
            catch (DeviceConnectionException e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.MonitorConnectionError,
                            e.Message,
                            e,
                            cycleNumber,
                            iterationNumber)));
            }
            catch (TimeoutRejectedException e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.PatientPressureParamsRequestTimeout,
                            e.Message,
                            e,
                            cycleNumber,
                            iterationNumber)));
            }
            catch (Exception e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.PatientPressureParamsRequestError,
                            e.Message,
                            e,
                            cycleNumber,
                            iterationNumber)));
            }
            finally
            {
                if (param == null)
                {
                    param = GetDefaultParams();
                }
            }


            UpdateContex(param, context);
            return context;
        }

        private void UpdateContex(Devices.Monitor.Infrastructure.PatientPressureParams param, CycleProcessingContext context)
        {
            context.AddOrUpdate(
                new PressureCycleProcessingContextParams(
                    param.SystolicArterialPressure,
                    param.DiastolicArterialPressure,
                    param.AverageArterialPressure));
        }

        private Devices.Monitor.Infrastructure.PatientPressureParams GetDefaultParams()
        {
            return new Devices.Monitor.Infrastructure.PatientPressureParams(-1, -1, -1);
        }

        public bool CanProcess([NotNull] CycleProcessingContext context)
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