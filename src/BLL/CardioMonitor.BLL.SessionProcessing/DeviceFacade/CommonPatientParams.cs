namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    internal class CommonPatientParams
    {
        public CommonPatientParams(
            float inclinationAngle,
            short heartRate, 
            short repsirationRate, 
            short spo2,
            short iterationNumber, 
            short cycleNumber)
        {
            HeartRate = heartRate;
            RepsirationRate = repsirationRate;
            Spo2 = spo2;
            IterationNumber = iterationNumber;
            CycleNumber = cycleNumber;
            InclinationAngle = inclinationAngle;
        }
        
        /// <summary>
        /// Угол наклона кровати
        /// </summary>
        public float InclinationAngle { get; }
        
        /// <summary>
        /// Номер итерации
        /// </summary>
        public short IterationNumber { get; }
        
        /// <summary>
        /// Номер цикла
        /// </summary>
        public short CycleNumber { get; }

        /// <summary>п
        /// Частота сердечных сокращений (ЧСС)
        /// </summary>
        public short HeartRate { get; }

        /// <summary>
        /// Частотат дыхания (ЧД)
        /// </summary>
        public short RepsirationRate { get;  }

        /// <summary>
        /// SPO2
        /// </summary>
        public short Spo2 { get;  }
    }
}