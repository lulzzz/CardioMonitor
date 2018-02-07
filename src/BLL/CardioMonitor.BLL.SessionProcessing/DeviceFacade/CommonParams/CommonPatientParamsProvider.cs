using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Angle;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.CheckPoints;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.ForcedDataCollectionRequest;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure.Threading;
using JetBrains.Annotations;

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
        [NotNull]
        private readonly TaskHelper _taskHelper;

        public CommonPatientParamsProvider(
            [NotNull] IMonitorController monitorController, 
            [NotNull] TaskHelper taskHelper)
        {
            _monitorController = monitorController ?? throw new ArgumentNullException(nameof(monitorController));
            _taskHelper = taskHelper ?? throw new ArgumentNullException(nameof(taskHelper));
            
            _updatePatientParamTimeout = new TimeSpan(0, 0, 8);
        }

        public async Task<CycleProcessingContext> ProcessAsync([NotNull] CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var angleParams = context.TryGetAngleParam();
            if (angleParams == null) return context;
            
            PatientCommonParams param = null;
            
            try
            {
                var gettingParamsTask = _monitorController.GetPatientCommonParamsAsync();
                param = await _taskHelper.StartWithTimeout(gettingParamsTask, _updatePatientParamTimeout);
            }
            catch (DeviceConnectionException e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.InversionTableConnectionError,
                            e.Message,
                            e)));
            }
            catch (TimeoutException e)
            {
              
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(SessionProcessingErrorCodes.PatientCommonParamsRequestTimeout,
                            e.Message,
                            e)));
            }
            catch (Exception e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(SessionProcessingErrorCodes.PatientCommonParamsRequestError,
                            e.Message,
                            e)));

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
    }
}