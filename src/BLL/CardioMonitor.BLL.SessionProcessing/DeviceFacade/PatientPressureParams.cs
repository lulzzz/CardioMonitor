namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    internal class PatientPressureParams
    {
        public PatientPressureParams(
            float inclinationAngle, 
            short systolicArterialPressure, 
            short diastolicArterialPressure, 
            short averageArterialPressure,
            short iterationNumber, 
            short cycleNumber)
        {
            InclinationAngle = inclinationAngle;
            SystolicArterialPressure = systolicArterialPressure;
            DiastolicArterialPressure = diastolicArterialPressure;
            AverageArterialPressure = averageArterialPressure;
            IterationNumber = iterationNumber;
            CycleNumber = cycleNumber;
        }

        /// <summary>
        /// Номер итерации
        /// </summary>
        public short IterationNumber { get; }
        
        /// <summary>
        /// Номер цикла
        /// </summary>
        public short CycleNumber { get; }
        
        /// <summary>
        /// Угол наклона кровати
        /// </summary>
        public float InclinationAngle { get; }
        
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