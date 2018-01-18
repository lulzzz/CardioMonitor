using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.CheckPoints;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Angle;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.CheckPoints
{
    internal class CheckPointChecker : ICycleProcessingPipelineElement
    {
        private readonly ICheckPointResolver _checkPointResolver;

        public CheckPointChecker([NotNull] ICheckPointResolver checkPointResolver)
        {
            _checkPointResolver = checkPointResolver ?? throw new ArgumentNullException(nameof(checkPointResolver));
        }

        public async Task<CycleProcessingContext> ProcessAsync([NotNull] CycleProcessingContext context)
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
                    context.AddOrUpdate(new CheckPointCycleProcessingContextParams(false, false));
                    return context;
                }
                var isMaxCheckPoint = _checkPointResolver.IsMaxCheckPointReached(angleParams.CurrentAngle);
                context.AddOrUpdate(new CheckPointCycleProcessingContextParams(true, isMaxCheckPoint));
            }
            catch (Exception e)
            {
                context.AddOrUpdate(
                    new ExceptionCycleProcessingContextParams(
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.Unknown,
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