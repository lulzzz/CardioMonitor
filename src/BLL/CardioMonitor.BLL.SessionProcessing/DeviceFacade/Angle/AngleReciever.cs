using System;
using System.Threading;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Iterations;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Time;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Bed.Infrastructure;
using JetBrains.Annotations;
using Markeli.Utils.Logging;
using Polly;
using Polly.Timeout;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.Angle
{
    internal class AngleReciever : ICycleProcessingPipelineElement
    {
        private readonly IBedController _bedController;
        private readonly TimeSpan _bedControllerTimeout;
        private ILogger _logger;

        private readonly SemaphoreSlim _mutex;
        private readonly TimeSpan _blockWaitingTimeout;
        
        public AngleReciever(
            [NotNull] IBedController bedController, 
            TimeSpan bedControllerTimeout)
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

            var sessionInfo = context.TryGetSessionProcessingInfo();
            var cycleNumber = sessionInfo?.CurrentCycleNumber;
            var iterationInfo = context.TryGetIterationParams();
            var iterationNumber = iterationInfo?.CurrentIteration;

            try
            {
               

                var timeoutPolicy = Policy.TimeoutAsync(_bedControllerTimeout);

                _logger?.Trace($"{GetType().Name}: запрос текущего угла наклона кровати по оси X с таймаутом {_bedControllerTimeout.TotalMilliseconds} мс");
                var currentAngle = await timeoutPolicy.ExecuteAsync(_bedController
                        .GetAngleXAsync)
                    .ConfigureAwait(false);

                context.AddOrUpdate(new AngleXContextParams(currentAngle));
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
                            SessionProcessingErrorCodes.UpdateAngleError,
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
                            SessionProcessingErrorCodes.UpdateAngleError,
                            e.Message,
                            e,
                            cycleNumber,
                            iterationNumber)));
            }
            finally
            {
                _mutex.Release();
            }
           
            return context;
        }

        public bool CanProcess([NotNull] CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return true;
        }

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }
    }
}