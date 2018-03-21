using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Time;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Bed.Infrastructure;
using JetBrains.Annotations;
using Polly;
using Polly.Timeout;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.Iterations
{
    internal class IterationParamsProvider : ICycleProcessingPipelineElement
    {
        [NotNull] private readonly IBedController _bedController;
        private readonly TimeSpan _bedControllerTimeout;

        public IterationParamsProvider([NotNull] IBedController bedController, 
            TimeSpan bedControllerTimeout)
        {
            _bedController = bedController;
            _bedControllerTimeout = bedControllerTimeout;
        }

        public async Task<CycleProcessingContext> ProcessAsync([NotNull] CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            
            var sessionInfo = context.TryGetSessionProcessingInfo();
            var cycleNumber = sessionInfo?.CurrentCycleNumber;
            
            try
            {
                var timeoutPolicy = Policy.TimeoutAsync(_bedControllerTimeout);
                var currentIteration = await timeoutPolicy
                    .ExecuteAsync(_bedController
                        .GetCurrentIterationAsync)
                    .ConfigureAwait(false);

                var nextIterationToMeasuringCommonParams = await timeoutPolicy
                    .ExecuteAsync(_bedController
                        .GetNextIterationNumberForCommonParamsMeasuringAsync)
                    .ConfigureAwait(false);

                var nextIterationToMeasuringPressureParams = await timeoutPolicy
                    .ExecuteAsync(_bedController
                        .GetNextIterationNumberForPressureMeasuringAsync)
                    .ConfigureAwait(false);

                var nextIterationToMeasuringEcg = await timeoutPolicy
                    .ExecuteAsync(_bedController
                        .GetNextIterationNumberForEcgMeasuringAsync)
                    .ConfigureAwait(false);

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
            return context;
        }

        public bool CanProcess([NotNull] CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return true;
        }
    }
}