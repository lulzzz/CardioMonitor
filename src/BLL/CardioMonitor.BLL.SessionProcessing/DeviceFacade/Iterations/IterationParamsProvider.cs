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
using Polly.Timeout;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.Iterations
{
    internal class IterationParamsProvider : ICycleProcessingPipelineElement
    {
        [NotNull] private readonly IBedController _bedController;
        private readonly TimeSpan _bedControllerTimeout;
        private ILogger _logger;
        private readonly object _lockObject;

        public IterationParamsProvider([NotNull] IBedController bedController, 
            TimeSpan bedControllerTimeout)
        {
            _bedController = bedController;
            _bedControllerTimeout = bedControllerTimeout;
            _lockObject = new object();
        }

        public async Task<CycleProcessingContext> ProcessAsync([NotNull] CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            
            var sessionInfo = context.TryGetSessionProcessingInfo();
            var cycleNumber = sessionInfo?.CurrentCycleNumber;
            
            try
            {
                if (!Monitor.TryEnter(_lockObject))
                {
                    _logger?.Warning($"{GetType().Name}: предыдущий запрос еще выполняется. Новый запрос не будет выполнен");
                    return context;
                }

                _logger?.Trace($"{GetType().Name}: запрос текущей итерации...");
                var timeoutPolicy = Policy.TimeoutAsync(_bedControllerTimeout);
                var currentIteration = await timeoutPolicy
                    .ExecuteAsync(_bedController
                        .GetCurrentIterationAsync)
                    .ConfigureAwait(false);
                _logger?.Trace($"{GetType().Name}: текущая итерация - {currentIteration}");

                _logger?.Trace($"{GetType().Name}: запрос следующей итерации для измерения общих параметров...");
                var nextIterationToMeasuringCommonParams = await timeoutPolicy
                    .ExecuteAsync(_bedController
                        .GetNextIterationNumberForCommonParamsMeasuringAsync)
                    .ConfigureAwait(false);
                _logger?.Trace($"{GetType().Name}: следующая итерация для измерения общих параметров - {nextIterationToMeasuringCommonParams}.");


                _logger?.Trace($"{GetType().Name}: запрос следующей итерации для измерения давления...");
                var nextIterationToMeasuringPressureParams = await timeoutPolicy
                    .ExecuteAsync(_bedController
                        .GetNextIterationNumberForPressureMeasuringAsync)
                    .ConfigureAwait(false);
                _logger?.Trace($"{GetType().Name}: следующая итерация для измерения давления - {nextIterationToMeasuringCommonParams}.");

                _logger?.Trace($"{GetType().Name}: запрос следующей итерации для измерения ЭКГ...");
                var nextIterationToMeasuringEcg = await timeoutPolicy
                    .ExecuteAsync(_bedController
                        .GetNextIterationNumberForEcgMeasuringAsync)
                    .ConfigureAwait(false);
                _logger?.Trace($"{GetType().Name}: следующая итерация для измерения ЭКГ - {nextIterationToMeasuringEcg}.");

                context.AddOrUpdate(new IterationCycleProcessingContextParams(
                    iterationToGetEcg: nextIterationToMeasuringEcg,
                    currentIteration: currentIteration,
                    iterationToGetCommonParams: nextIterationToMeasuringCommonParams,
                    iterationToGetPressureParams: nextIterationToMeasuringPressureParams));
            }
            catch (DeviceConnectionException e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.InversionTableConnectionError,
                            e.Message,
                            e,
                            cycleNumber)));
            }
            catch (TimeoutRejectedException e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.InversionTableProcessingError,
                            e.Message,
                            e,
                            cycleNumber)));
            }
            catch (Exception e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.InversionTableProcessingError,
                            e.Message,
                            e,
                            cycleNumber)));
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