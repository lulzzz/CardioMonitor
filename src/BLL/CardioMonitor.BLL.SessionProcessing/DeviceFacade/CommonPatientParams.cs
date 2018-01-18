namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    internal class CommonPatientParams
    {
        public CommonPatientParams(
            double inclinationAngle,
            short heartRate, 
            short repsirationRate, 
            short spo2)
        {
            HeartRate = heartRate;
            RepsirationRate = repsirationRate;
            Spo2 = spo2;
            InclinationAngle = inclinationAngle;
        }
        
        /// <summary>
        /// Угол наклона кровати
        /// </summary>
        public double InclinationAngle { get; }

        /// <summary>
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