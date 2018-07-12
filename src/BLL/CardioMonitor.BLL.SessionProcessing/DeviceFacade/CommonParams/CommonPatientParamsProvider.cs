using System;
using System.Threading;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Angle;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.CheckPoints;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.ForcedDataCollectionRequest;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Iterations;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.SessionProcessingInfo;
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
        private readonly SemaphoreSlim _mutex;
        private readonly TimeSpan _blockWaitingTimeout;

        public CommonPatientParamsProvider(
            [NotNull] IMonitorController monitorController,
            TimeSpan updatePatientParamTimeout)
        {
            _monitorController = monitorController ?? throw new ArgumentNullException(nameof(monitorController));
            _updatePatientParamTimeout = updatePatientParamTimeout;
            _mutex = new SemaphoreSlim(1);
            // считаем стандартным период обновления данных в Pipeline 1 секунду, 
            // если за пол секунлы этот метод не выполнился, что-то идет не так 
            _blockWaitingTimeout = TimeSpan.FromMilliseconds(500);
        }

        public async Task<CycleProcessingContext> ProcessAsync([NotNull] CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var angleParams = context.TryGetAngleParam();
            if (angleParams == null)
            {
                var forcedRequest = context.TryGetForcedDataCollectionRequest();
                forcedRequest?.CommonParamsSemaphore.SetResult(true);
                return context;
            }
            
            var isFree = await _mutex
                .WaitAsync(_blockWaitingTimeout)
                .ConfigureAwait(false);
            if (!isFree)
            {
                _logger?.Warning($"{GetType().Name}: предыдущий запрос еще выполняется. " +
                                 $"Новый запрос не будет выполнен, т.к. прошло больше {_blockWaitingTimeout.TotalMilliseconds} мс");

                var forcedRequest = context.TryGetForcedDataCollectionRequest();
                forcedRequest?.CommonParamsSemaphore.SetResult(true);
                return context;
            }

            PatientCommonParams param = null;

            var sessionInfo = context.TryGetSessionProcessingInfo();
            var cycleNumber = sessionInfo?.CurrentCycleNumber;
            var iterationInfo = context.TryGetIterationParams();
            var iterationNumber = iterationInfo?.CurrentIteration;

            try
            {              
                _logger?.Trace($"{GetType().Name}: запрос общих параметров пациента с таймаутом {_updatePatientParamTimeout.TotalMilliseconds} мс");
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
                var forcedRequest = context.TryGetForcedDataCollectionRequest();
                forcedRequest?.CommonParamsSemaphore.SetResult(true);
                _mutex.Release();
                if (param == null)
                {
                    param = new PatientCommonParams(-1, -1, -1);
                }
            }
            
            _logger?.Trace($"{GetType().Name}: текущие общие показатели: ЧСС - {param.HeartRate}, " +
                           $"ЧСД - {param.RepsirationRate}, " +
                           $"SPO2 - {param.Spo2}");
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
            if (forcedRequest != null)
            {
                return true;
            }
            
            var checkPointReachedParams = context.TryGetCheckPointParams();
            return checkPointReachedParams != null && checkPointReachedParams.NeedRequestCommonParams;
        }

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }
    }
}