using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.SessionProcessing.CycleProcessing.Angle;
using CardioMonitor.BLL.SessionProcessing.CycleProcessing.CheckPoints;
using CardioMonitor.BLL.SessionProcessing.CycleProcessing.Exceptions;
using CardioMonitor.BLL.SessionProcessing.CycleProcessing.ForcedDataCollectionRequest;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure.Threading;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.CycleProcessing.CommonParams
{
    internal class CommonPatientParamsProvider : ICycleProcessingPipelineElement
    {
        /// <summary>
        /// Точность для сравнение double величин
        /// </summary>
        private const double Tolerance = 0.1e-12;


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
            
            PatientParams param = null;
            
            try
            {
                var gettingParamsTask = _monitorController.GetPatientParamsAsync();
                param = await _taskHelper.StartWithTimeout(gettingParamsTask, _updatePatientParamTimeout);
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
                    param = new PatientParams
                    {
                        RepsirationRate = -1,
                        HeartRate = -1,
                        Spo2 = -1,
                        SystolicArterialPressure = -1,
                        DiastolicArterialPressure = -1,
                        AverageArterialPressure = -1
                    };
                }
            }
            param.InclinationAngle = Math.Abs(angleParams.CurrentAngle) < Tolerance ? 0 : angleParams.CurrentAngle;
            
            context.AddOrUpdate(
                new CommonPatientCycleProcessingContextParams(
                    param.InclinationAngle,
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
            return checkPointReachedParams != null && checkPointReachedParams.IsCheckPointReached;
        }
    }
}