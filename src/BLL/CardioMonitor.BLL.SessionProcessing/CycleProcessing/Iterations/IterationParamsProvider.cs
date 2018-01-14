using System;
using System.Threading.Tasks;
using CardioMonitor.Devices.Bed.Infrastructure;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.CycleProcessing.Iterations
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

            await Task.Yield();
            
            //todo request of iteration params
            //var _bedController.get

            context.AddOrUpdate(new IterationCycleProcessingContextParams(
                iterationToGetEcg: 0,
                currentIteration: 0,
                iterationToGetCommonParams: 0,
                iterationToGetPressureParams: 0));
            
            return context;
        }

        public bool CanProcess([NotNull] CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return true;
        }
    }
}