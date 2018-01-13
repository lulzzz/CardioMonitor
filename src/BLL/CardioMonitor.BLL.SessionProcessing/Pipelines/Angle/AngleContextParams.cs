using System;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines.Angle
{
    public class AngleContextParams : IContextParams
    {
        public static readonly Guid AngleContextParamId = new Guid("7f2ae094-ea77-407e-9611-9b14a3fc2bbd");

        public Guid ParamsTypeId { get; } = AngleContextParamId;

        public AngleContextParams(double currentAngle)
        {
            CurrentAngle = currentAngle;
        }

        public double CurrentAngle { get; }
    }

    public static class AngleParamContextExtension
    {
        public static AngleContextParams TryGetAngleParam([NotNull] this PipelineContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            return context.TryGet(AngleContextParams.AngleContextParamId) as AngleContextParams;
        }
    }
}