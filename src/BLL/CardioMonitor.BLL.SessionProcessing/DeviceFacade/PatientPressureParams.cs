﻿namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    internal class PatientPressureParams
    {
        public PatientPressureParams(
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
}