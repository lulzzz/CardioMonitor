using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.SessionProcessing.Pipelines.ActionBlocks;
using CardioMonitor.BLL.SessionProcessing.Pipelines.Angle;
using CardioMonitor.BLL.SessionProcessing.Pipelines.CheckPoints;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure.Threading;
using Enexure.MicroBus.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines.PressureParams
{
    public class PatientPressureParamsProvider : IPipelineElement
    {
         /// <summary>
        /// Точность для сравнение double величин
        /// </summary>
        private const double Tolerance = 0.1e-12;

        /// <summary>
        /// Таймаут операции накачки
        /// </summary>
        private readonly TimeSpan _pumpingTimeout;

        /// <summary>
        /// Таймаут запроса параметром пациента
        /// </summary>
        private readonly TimeSpan _updatePatientParamTimeout;
        [NotNull]
        private readonly IMonitorController _monitorController;
        [NotNull]
        private readonly TaskHelper _taskHelper;

        public PatientPressureParamsProvider(
            [NotNull] IMonitorController monitorController, 
            [NotNull] TaskHelper taskHelper)
        {
            _monitorController = monitorController ?? throw new ArgumentNullException(nameof(monitorController));
            _taskHelper = taskHelper ?? throw new ArgumentNullException(nameof(taskHelper));
            
            _updatePatientParamTimeout = new TimeSpan(0, 0, 8);
            
            _pumpingTimeout = new TimeSpan(0, 0, 8);
        }

        public async Task<PipelineContext> ProcessAsync([NotNull] PipelineContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var angleParams = context.TryGetAngleParam();
            if (angleParams == null) return context;
            
            PatientParams param;
            try
            {
                var pumpingTask = _monitorController.PumpCuffAsync();
                var isSuccessfully = await _taskHelper.StartWithTimeout(pumpingTask, _pumpingTimeout);
                if (!isSuccessfully)
                {
                    return context;
                }
                
                var gettingParamsTask = _monitorController.GetPatientParamsAsync();
                param = await _taskHelper.StartWithTimeout(gettingParamsTask, _updatePatientParamTimeout);
            }
            catch (TimeoutException)
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
            catch (Exception)
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
            param.InclinationAngle = Math.Abs(angleParams.CurrentAngle) < Tolerance ? 0 : angleParams.CurrentAngle;
            
            context.AddOrUpdate(
                new PressureContextParams(
                    param.InclinationAngle,
                    param.SystolicArterialPressure,
                    param.DiastolicArterialPressure,
                    param.AverageArterialPressure));

            return context;
        }

        public bool CanProcess([NotNull] PipelineContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var checkPointReachedParams = context.TryGetCheckPointParams();
            return checkPointReachedParams != null && checkPointReachedParams.IsCheckPointReached;
        }
    }
}