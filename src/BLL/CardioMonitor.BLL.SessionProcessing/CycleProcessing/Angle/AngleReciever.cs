using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices.Bed.Infrastructure;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.CycleProcessing.Angle
{
    internal class AngleReciever : IPipelineElement
    {
        private readonly IBedController _bedController;
        

        public AngleReciever([NotNull] IBedController bedController)
        {
            _bedController = bedController ?? throw new ArgumentNullException(nameof(bedController));
        }
        
        public async Task<PipelineContext> ProcessAsync([NotNull] PipelineContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                var currentAngle = await _bedController
                    .GetAngleXAsync()
                    .ConfigureAwait(false);
            
                context.AddOrUpdate(new AngleContextParams(currentAngle));
            }
            catch (Exception e)
            {
                context.AddOrUpdate(
                    new ExceptionContextParams(
                        new SessionProcessingException(SessionProcessingErrorCodes.UpdateAngleError, e.Message, e)));
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