using System;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines.PressureParams
{
    public class PressureContextParams : IContextParams
    {
        public static readonly Guid PressureParamsId = new Guid("59c3d092-78d1-4e6e-b3d5-22a4ca3d298c");
        
        public Guid ParamsTypeId { get; }

        public PressureContextParams(
            double inclinationAngle, 
            short systolicArterialPressure, 
            short diastolicArterialPressure, 
            short averageArterialPressure)
        {
            InclinationAngle = inclinationAngle;
            SystolicArterialPressure = systolicArterialPressure;
            DiastolicArterialPressure = diastolicArterialPressure;
            AverageArterialPressure = averageArterialPressure;
        }

        /// <summary>
        /// Угол наклона кровати
        /// </summary>
        public double InclinationAngle { get; }
        
        /// <summary>
        /// Систолическое артериальное давление
        /// </summary>
        public short SystolicArterialPressure { get; }

        /// <summary>
        /// Диастолическое артериальное давление
        /// </summary>
        public short DiastolicArterialPressure { get; }

        /// <summary>
        /// Среднее артериальное давлние 
        /// </summary>
        public short AverageArterialPressure { get;  }
    }

    public static class PressureParamsContextExntensions
    {
        public static PressureContextParams TryGetPressureParams([NotNull] this PipelineContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.TryGet(PressureContextParams.PressureParamsId) as PressureContextParams;
        }
    }
}