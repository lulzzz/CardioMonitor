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
        [NotNull]
        private readonly IBedController _bedController;

        private readonly TimeSpan _bedControllerTimeout;
        private ILogger _logger;
        private readonly object _lockObject;

        public SessionProcessingInfoProvider([NotNull] IBedController bedController, TimeSpan bedControllerTimeout)
        {
            _bedController = bedController ?? throw new ArgumentNullException(nameof(bedController));
            _bedControllerTimeout = bedControllerTimeout;
            _lockObject = new object();
        }

        public async Task<CycleProcessingContext> ProcessAsync(CycleProcessingContext context)
        {
            try
            {
                if (!Monitor.TryEnter(_lockObject))
                {
                    _logger?.Warning($"{GetType().Name}: предыдущий запрос еще выполняется. Новый запрос не будет выполнен");
                    return context;
                }

                var timeoutPolicy = Policy.TimeoutAsync(_bedControllerTimeout);

                _logger?.Trace($"{GetType().Name}: запрос прошедшего времени");
                var elapsedTime = await timeoutPolicy
                    .ExecuteAsync(
                        _bedController.GetElapsedTimeAsync)
                    .ConfigureAwait(false);

                _logger?.Trace($"{GetType().Name}: запрос оставшегося времени");
                var remainingTime = await timeoutPolicy
                    .ExecuteAsync(
                        _bedController .GetRemainingTimeAsync)
                    .ConfigureAwait(false);

                _logger?.Trace($"{GetType().Name}: запрос длительности цикла");
                var cycleDuration = await timeoutPolicy
                    .ExecuteAsync(
                        _bedController.GetCycleDurationAsync)
                    .ConfigureAwait(false);

                _logger?.Trace($"{GetType().Name}: запрос количества циклов");
                var cyclesCount = await timeoutPolicy
                    .ExecuteAsync(
                        _bedController.GetCyclesCountAsync)
                    .ConfigureAwait(false);

                _logger?.Trace($"{GetType().Name}: запрос номера текущего цикла");
                var currentCycleNumber = await timeoutPolicy
                    .ExecuteAsync(
                        _bedController.GetCurrentCycleNumberAsync)
                    .ConfigureAwait(false);

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
                if (Monitor.IsEntered(_lockObject))
                {
                    Monitor.Exit(_lockObject);
                }
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