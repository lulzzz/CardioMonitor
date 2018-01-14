using System;
using CardioMonitor.BLL.SessionProcessing.CycleProcessing.Exceptions;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.CycleProcessing.PressureParams
{
    internal class PressureCycleProcessingContextParams : ICycleProcessingContextParams
    {
        public static readonly Guid PressureParamsId = new Guid("59c3d092-78d1-4e6e-b3d5-22a4ca3d298c");
        
        public Guid ParamsTypeId { get; }

        public PressureCycleProcessingContextParams(
            short systolicArterialPressure, 
            short diastolicArterialPressure, 
            short averageArterialPressure)
        {
            SystolicArterialPressure = systolicArterialPressure;
            DiastolicArterialPressure = diastolicArterialPressure;
            AverageArterialPressure = averageArterialPressure;
        }
        
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

    internal static class PressureParamsContextExntensions
    {
        public static PressureCycleProcessingContextParams TryGetPressureParams([NotNull] this CycleProcessingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.TryGet(PressureCycleProcessingContextParams.PressureParamsId) as PressureCycleProcessingContextParams;
        }
    }
}