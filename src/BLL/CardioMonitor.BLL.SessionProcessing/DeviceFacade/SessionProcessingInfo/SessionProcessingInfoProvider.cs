using System;
using System.Threading;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Time;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Bed.Infrastructure;
using JetBrains.Annotations;
using Markeli.Utils.Logging;
using Polly;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.SessionProcessingInfo
{
    /// <summary>
    /// Поставщик общей информации о сеансе
    /// </summary>
    internal class SessionProcessingInfoProvider : ICycleProcessingPipelineElement
    {
        private const short StartCycleNumber = 1;

        [NotNull]
        private readonly IBedController _bedController;

        private readonly TimeSpan _bedControllerTimeout;
        private ILogger _logger;

        private readonly SemaphoreSlim _mutex;
        private readonly TimeSpan _blockWaitingTimeout;

        public SessionProcessingInfoProvider([NotNull] IBedController bedController, TimeSpan bedControllerTimeout)
        {
            _bedController = bedController ?? throw new ArgumentNullException(nameof(bedController));
            _bedControllerTimeout = bedControllerTimeout;

            _mutex = new SemaphoreSlim(1);
            // считаем стандартным период обновления данных в Pipeline 1 секунду, 
            // если за пол секунлы этот метод не выполнился, что-то идет не так 
            _blockWaitingTimeout = TimeSpan.FromMilliseconds(500);
        }

        public async Task<CycleProcessingContext> ProcessAsync([NotNull] CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var isBlocked = await _mutex
                .WaitAsync(_blockWaitingTimeout)
                .ConfigureAwait(false);
            if (!isBlocked)
            {
                _logger?.Warning($"{GetType().Name}: предыдущий запрос еще выполняется. " +
                                 $"Новый запрос не будет выполнен, т.к. прошло больше {_blockWaitingTimeout.TotalMilliseconds} мс");
                return context;
            }

            try
            {

                var timeoutPolicy = Policy.TimeoutAsync(_bedControllerTimeout);

                _logger?.Trace($"{GetType().Name}: запрос прошедшего времени с тайаутом {_bedControllerTimeout.Milliseconds} мс");
                var elapsedTime = await timeoutPolicy
                    .ExecuteAsync(
                        _bedController.GetElapsedTimeAsync)
                    .ConfigureAwait(false);

                _logger?.Trace($"{GetType().Name}: запрос оставшегося времени с тайаутом {_bedControllerTimeout.Milliseconds} мс");
                var remainingTime = await timeoutPolicy
                    .ExecuteAsync(
                        _bedController.GetRemainingTimeAsync)
                    .ConfigureAwait(false);

                _logger?.Trace($"{GetType().Name}: запрос длительности цикла с тайаутом {_bedControllerTimeout.Milliseconds} мс");
                var cycleDuration = await timeoutPolicy
                    .ExecuteAsync(
                        _bedController.GetCycleDurationAsync)
                    .ConfigureAwait(false);

                _logger?.Trace($"{GetType().Name}: запрос количества циклов с тайаутом {_bedControllerTimeout.Milliseconds} мс");
                var cyclesCount = await timeoutPolicy
                    .ExecuteAsync(
                        _bedController.GetCyclesCountAsync)
                    .ConfigureAwait(false);

                _logger?.Trace($"{GetType().Name}: запрос номера текущего цикла с тайаутом {_bedControllerTimeout.Milliseconds} мс");
                var currentCycleNumber = await timeoutPolicy
                    .ExecuteAsync(
                        _bedController.GetCurrentCycleNumberAsync)
                    .ConfigureAwait(false);
                if (currentCycleNumber == 0)
                {
                    currentCycleNumber = StartCycleNumber;
                }

                var sessionProcessingInfo = new SessionProcessingInfoContextParamses(
                    elapsedTime,
                    remainingTime,
                    cycleDuration,
                    currentCycleNumber,
                    cyclesCount);

                context.AddOrUpdate(sessionProcessingInfo);
            }
            catch (DeviceConnectionException ex)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(SessionProcessingErrorCodes.InversionTableConnectionError,
                            ex.Message,
                            ex)));
            }
            catch (DeviceProcessingException ex)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(SessionProcessingErrorCodes.InversionTableProcessingError,
                            ex.Message,
                            ex)));

            }
            catch (Exception ex)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(SessionProcessingErrorCodes.InversionTableProcessingError,
                            ex.Message,
                            ex)));

            }
            finally
            {
                _mutex.Release();
            }

            return context;
        }

        public bool CanProcess(CycleProcessingContext context)
        {
            return true;
        }

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }
    }
}