using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Bed.Infrastructure;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.Iterations
{
    internal class IterationParamsProvider : ICycleProcessingPipelineElement
    {
        [NotNull] private readonly IBedController _bedController;

        public IterationParamsProvider([NotNull] IBedController bedController)
        {
            _bedController = bedController;
        }

        public async Task<CycleProcessingContext> ProcessAsync([NotNull] CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                var currentIteration = await _bedController
                    .GetCurrentIterationAsync()
                    .ConfigureAwait(false);

                var nextIterationToMeasuringCommonParams = await _bedController
                    .GetNextIterationNumberForCommonParamsMeasuringAsync()
                    .ConfigureAwait(false);

                var nextIterationToMeasuringPressureParams = await _bedController
                    .GetNextIterationNumberForPressureMeasuringAsync()
                    .ConfigureAwait(false);

                var nextIterationToMeasuringEcg = await _bedController
                    .GetNextIterationNumberForEcgMeasuringAsync()
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
                            e)));
            }
            catch (Exception e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.InversionTableProcessingError,
                            e.Message,
                            e)));
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