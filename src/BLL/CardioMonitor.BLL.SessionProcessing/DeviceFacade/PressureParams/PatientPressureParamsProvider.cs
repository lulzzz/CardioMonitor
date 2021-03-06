﻿using System;
using System.Threading;
using System.Threading.Tasks;
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

        private readonly SemaphoreSlim _mutex;
        private readonly TimeSpan _blockWaitingTimeout;

        public PatientPressureParamsProvider(
            [NotNull] IMonitorController monitorController,
            TimeSpan operationsTimeout)
        {
            _monitorController = monitorController ?? throw new ArgumentNullException(nameof(monitorController));
            
            _updatePatientParamTimeout = operationsTimeout;

            _mutex = new SemaphoreSlim(1);
            // считаем стандартным период обновления данных в Pipeline 1 секунду, 
            // если за пол секунлы этот метод не выполнился, что-то идет не так 
            _blockWaitingTimeout = TimeSpan.FromMilliseconds(500);
        }

        public async Task<CycleProcessingContext> ProcessAsync([NotNull] CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            if (!context.IsValid())
            {
                _logger.Warning($"{GetType().Name}: действие не будет выполнено, т.к. в обработке сеанса возникли ошибки");
                return context;
            }
            
            var pumpingResult = context.TryGetAutoPumpingResultParams();
            if (!(pumpingResult?.WasPumpingCompleted ?? false))
            {
                _logger?.Trace($"{GetType().Name}: получение данных не будет выполнено, " +
                               $"т.к. накачка манжеты завершилась неудачно");
                var forcedRequest = context.TryGetForcedDataCollectionRequest();
                forcedRequest?.PressureParamsSemaphore.SetResult(true);
                return context;
            }

            var isFree = await _mutex
                .WaitAsync(_blockWaitingTimeout)
                .ConfigureAwait(false);
            if (!isFree)
            {
                _logger?.Warning($"{GetType().Name}: предыдущий запрос еще выполняется. " +
                                 $"Новый запрос не будет выполнен, т.к. прошло больше " +
                                 $"{_blockWaitingTimeout.TotalMilliseconds} мс");
                var forcedRequest = context.TryGetForcedDataCollectionRequest();
                forcedRequest?.PressureParamsSemaphore.SetResult(true);
                return context;
            }

            Devices.Monitor.Infrastructure.PatientPressureParams param = null;
            
            var sessionInfo = context.TryGetSessionProcessingInfo();
            var cycleNumber = sessionInfo?.CurrentCycleNumber;
            var iterationInfo = context.TryGetIterationParams();
            var iterationNumber = iterationInfo?.CurrentIteration;

            try
            {


                _logger?.Trace($"{GetType().Name}: запрос показателей давления с таймаутом " +
                               $"{_updatePatientParamTimeout.TotalMilliseconds} мс");
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
                            "Получение показателей давления пациента прервано по таймауту",
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
                var forcedRequest = context.TryGetForcedDataCollectionRequest();
                forcedRequest?.PressureParamsSemaphore.SetResult(true);
                _mutex.Release();
                if (param == null)
                {
                    param = GetDefaultParams();
                }
            }

            _logger?.Trace($"{GetType().Name}: текущие показатели давления: систолическиое - {param.SystolicArterialPressure}, " +
                           $"диастолическое - {param.DiastolicArterialPressure}, " +
                           $"среднее - {param.AverageArterialPressure}");
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
            if (forcedRequest != null)
            {
                return true;
            }
            
            var checkPointReachedParams = context.TryGetCheckPointParams();
            return checkPointReachedParams != null && checkPointReachedParams.NeedRequestPressureParams;
        }

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }
    }
}