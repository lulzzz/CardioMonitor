using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CardioMonitor.BLL.SessionProcessing.Pipelines.ActionBlocks;
using CardioMonitor.Devices.Bed.Infrastructure;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines.Angle
{
    public class AngleReciever : IPipelineElement
    {
        private readonly IBedController _bedController;
        

        public AngleReciever([NotNull] IBedController bedController)
        {
            _bedController = bedController ?? throw new ArgumentNullException(nameof(bedController));
        }
        
        public async Task<PipelineContext> ProcessAsync([NotNull] PipelineContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            var currentAngle = await _bedController
                .GetAngleXAsync()
                .ConfigureAwait(false);
            
            context.AddOrUpdate(new AngleContextParams(currentAngle));
            return context;
        }

        public bool CanProcess([NotNull] PipelineContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return true;
        }
    }
}