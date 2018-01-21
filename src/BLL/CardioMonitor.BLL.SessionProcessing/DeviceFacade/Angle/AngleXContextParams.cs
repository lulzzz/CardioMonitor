using System;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.Angle
{
    internal class AngleXContextParams : ICycleProcessingContextParams
    {
        public static readonly Guid AngleContextParamId = new Guid("7f2ae094-ea77-407e-9611-9b14a3fc2bbd");

        public Guid ParamsTypeId { get; } = AngleContextParamId;
        
        public Guid UniqObjectId { get; }

        public AngleXContextParams(float currentAngle)
        {
            CurrentAngle = currentAngle;
            UniqObjectId = Guid.NewGuid();
        }

        public float CurrentAngle { get; }
    }

    internal static class AngleXParamContextExtension
    {
        public static AngleXContextParams TryGetAngleParam([NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            return context.TryGet(AngleXContextParams.AngleContextParamId) as AngleXContextParams;
        }
    }
}