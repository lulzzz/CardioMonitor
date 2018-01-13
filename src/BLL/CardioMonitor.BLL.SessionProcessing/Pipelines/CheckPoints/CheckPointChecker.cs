using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.CheckPoints;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.BLL.SessionProcessing.Pipelines.Angle;
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

            try
            {
                var isCheckPointReached = _checkPointResolver.IsCheckPointReached(angleParams.CurrentAngle);
                if (!isCheckPointReached)
                {
                    context.AddOrUpdate(new CheckPointContextParams(false, false));
                    return context;
                }
                var isMaxCheckPoint = _checkPointResolver.IsMaxCheckPointReached(angleParams.CurrentAngle);
                context.AddOrUpdate(new CheckPointContextParams(true, isMaxCheckPoint));
            }
            catch (Exception e)
            {
                context.AddOrUpdate(
                    new ExceptionContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.Unknown,
                            e.Message,
                            e)));
            }
            
           
            
            return context;
        }

        public bool CanProcess([NotNull] PipelineContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return true;
        }
    }
}