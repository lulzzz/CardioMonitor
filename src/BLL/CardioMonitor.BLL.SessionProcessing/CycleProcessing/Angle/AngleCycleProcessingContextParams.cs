using System;
using CardioMonitor.BLL.SessionProcessing.CycleProcessing.Exceptions;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.CycleProcessing.Angle
{
    internal class AngleCycleProcessingContextParams : ICycleProcessingContextParams
    {
        public static readonly Guid AngleContextParamId = new Guid("7f2ae094-ea77-407e-9611-9b14a3fc2bbd");

        public Guid ParamsTypeId { get; } = AngleContextParamId;

        public AngleCycleProcessingContextParams(double currentAngle)
        {
            CurrentAngle = currentAngle;
        }

        public double CurrentAngle { get; }
    }

    internal static class AngleParamContextExtension
    {
        public static AngleCycleProcessingContextParams TryGetAngleParam([NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            return context.TryGet(AngleCycleProcessingContextParams.AngleContextParamId) as AngleCycleProcessingContextParams;
        }
    }
}