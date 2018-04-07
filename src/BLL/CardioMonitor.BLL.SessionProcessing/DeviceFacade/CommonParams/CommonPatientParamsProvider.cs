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

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.CommonParams
{
    internal class CommonPatientParamsProvider : ICycleProcessingPipelineElement
    {
        /// <summary>
        /// Таймаут запроса параметром пациента
        /// </summary>
        private readonly TimeSpan _updatePatientParamTimeout;
        [NotNull]
        private readonly IMonitorController _monitorController;

        private ILogger _logger;

        public CommonPatientParamsProvider(
            [NotNull] IMonitorController monitorController,
            TimeSpan updatePatientParamTimeout)
        {
            _monitorController = monitorController ?? throw new ArgumentNullException(nameof(monitorController));
            _updatePatientParamTimeout = updatePatientParamTimeout;
        }

        public async Task<CycleProcessingContext> ProcessAsync([NotNull] CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var angleParams = context.TryGetAngleParam();
            if (angleParams == null) return context;
            
            PatientCommonParams param = null;
            
            var sessionInfo = context.TryGetSessionProcessingInfo();
            var cycleNumber = sessionInfo?.CurrentCycleNumber;
            var iterationInfo = context.TryGetIterationParams();
            var iterationNumber = iterationInfo?.CurrentIteration;
            
            try
            {
                _logger?.Trace($"{GetType().Name}: запрос общих параметров пациента");
                var timeoutPolicy = Policy.TimeoutAsync(_updatePatientParamTimeout);
                param = await timeoutPolicy
                    .ExecuteAsync(_monitorController.GetPatientCommonParamsAsync)
                    .ConfigureAwait(false);
            }
            catch (DeviceConnectionException e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.InversionTableConnectionError,
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
                            SessionProcessingErrorCodes.PatientCommonParamsRequestTimeout,
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
                            SessionProcessingErrorCodes.PatientCommonParamsRequestError,
                            e.Message,
                            e,
                            cycleNumber,
                            iterationNumber)));

            }
            finally
            {
                if (param == null)
                {
                    param = new PatientCommonParams(-1, -1, -1);
                }
            }
            
            context.AddOrUpdate(
                new CommonPatientCycleProcessingContextParams(
                    param.HeartRate,
                    param.RepsirationRate,
                    param.Spo2));

            return context;
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