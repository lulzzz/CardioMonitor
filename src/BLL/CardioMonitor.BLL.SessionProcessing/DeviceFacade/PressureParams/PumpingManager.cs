﻿using System;
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
using Polly.Timeout;

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

        private readonly SemaphoreSlim _mutex;
        private readonly TimeSpan _blockWaitingTimeout;
        
        private readonly TimeSpan _defaultRetryTimeout = TimeSpan.FromSeconds(1);

        public PumpingManager([NotNull] IMonitorController monitorController,
            TimeSpan pumpingTimeout)
        {
            _monitorController = monitorController ?? throw new ArgumentNullException(nameof(monitorController));
            
            _pumpingTimeout = pumpingTimeout;
            _mutex = new SemaphoreSlim(1);
            // считаем стандартным период обновления данных в Pipeline 1 секунду, 
            // если за пол секунлы этот метод не выполнился, что-то идет не так 
            _blockWaitingTimeout = TimeSpan.FromMilliseconds(500);
        }

        public async Task<CycleProcessingContext> ProcessAsync(CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            if (!context.IsValid())
            {
                _logger.Warning($"{GetType().Name}: действие не будет выполнено, т.к. в обработке сеанса возникли ошибки");
                return context;
            }
            
            var needPumping = context.TryGetAutoPumpingRequestParams()?.IsAutoPumpingEnabled ?? false;

            _logger?.Trace($"{GetType().Name}: накачка манжеты...");
            if (!needPumping)
            {
                _logger?.Trace($"{GetType().Name}: накачка манжеты не требуется");
                context.AddOrUpdate(new PumpingResultContextParams(true));
                return context;
            }

            var isFree = await _mutex
                .WaitAsync(_blockWaitingTimeout)
                .ConfigureAwait(false);
            if (!isFree)
            {
                _logger?.Warning($"{GetType().Name}: предыдущий запрос еще выполняется. " +
                                 $"Новый запрос не будет выполнен, т.к. прошло больше {_blockWaitingTimeout.TotalMilliseconds} мс");
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
                    .WaitAndRetryAsync(
                        retryCounts,
                        retryAttemp => TimeSpan.FromSeconds(_defaultRetryTimeout.TotalSeconds),
                        (exception, timeSpan, localContext) =>
                        {
                            _logger?.Trace($"{GetType().Name}: накачнка манжеты не выполена. Будет выполнена " +
                                           "повторная попытка.");
                        });
                var policyWrap = Policy.WrapAsync(recilencePolicy, timeoutPolicy);
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
            catch (TimeoutRejectedException e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.PumpingTimeout,
                            "Накачка манжеты прервана по таймауту",
                            e)));
                wasPumpingComleted = false;
            }
            catch (DeviceProcessingException e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.PumpingError,
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
                _mutex.Release();
            }

            context.AddOrUpdate(new PumpingResultContextParams(wasPumpingComleted));

            return context;
        }

        public bool CanProcess(CycleProcessingContext context)
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