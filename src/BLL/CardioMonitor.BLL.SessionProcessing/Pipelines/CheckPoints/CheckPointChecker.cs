using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.Pipelines.ActionBlocks;
using CardioMonitor.BLL.SessionProcessing.Pipelines.Angle;
using CardioMonitor.SessionProcessing;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines.CheckPoints
{
    internal class CheckPointChecker : IPipelineElement
    {
        private readonly ICheckPointResolver _checkPointResolver;

        public CheckPointChecker([NotNull] ICheckPointResolver checkPointResolver)
        {
            _checkPointResolver = checkPointResolver ?? throw new ArgumentNullException(nameof(checkPointResolver));
        }

        public async Task<PipelineContext> ProcessAsync([NotNull] PipelineContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            await Task.Yield();
            var angleParams = context.TryGetAngleParam();
            if (angleParams == null) return context;

            var isCheckPointReached = _checkPointResolver.IsCheckPointReached(angleParams.CurrentAngle);
            if (!isCheckPointReached)
            {
                context.AddOrUpdate(new CheckPointContextParams(false, false));
                return context;
            }
            var isMaxCheckPoint = _checkPointResolver.IsMaxCheckPointReached(angleParams.CurrentAngle);
            context.AddOrUpdate(new CheckPointContextParams(true, isMaxCheckPoint));
            
            return context;
        }

        public bool CanProcess([NotNull] PipelineContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return true;
        }
    }
}