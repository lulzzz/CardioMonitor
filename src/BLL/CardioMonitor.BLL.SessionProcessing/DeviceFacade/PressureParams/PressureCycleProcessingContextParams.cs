using System;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade.PressureParams
{
    internal class PressureCycleProcessingContextParams : ICycleProcessingContextParams
    {
        public static readonly Guid PressureParamsId = new Guid("59c3d092-78d1-4e6e-b3d5-22a4ca3d298c");

        public Guid ParamsTypeId { get; } = PressureParamsId;
        public Guid UniqObjectId { get; }

        public PressureCycleProcessingContextParams(
            short systolicArterialPressure, 
            short diastolicArterialPressure, 
            short averageArterialPressure)
        {
            SystolicArterialPressure = systolicArterialPressure;
            DiastolicArterialPressure = diastolicArterialPressure;
            AverageArterialPressure = averageArterialPressure;
            UniqObjectId = Guid.NewGuid();
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
}