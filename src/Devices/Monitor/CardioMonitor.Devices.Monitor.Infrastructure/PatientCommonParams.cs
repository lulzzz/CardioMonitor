namespace CardioMonitor.Devices.Monitor.Infrastructure
{
    /// <summary>
    /// Общиие параметры пациента 
    /// </summary>
    /// <remarks>
    /// Все, за исключением давления и ЭКГ
    /// </remarks>
    public class PatientCommonParams
    {
        public PatientCommonParams(
            short heartRate, 
            short repsirationRate, 
            short spo2)
        {
            HeartRate = heartRate;
            RepsirationRate = repsirationRate;
            Spo2 = spo2;
        }

        /// <summary>
        /// Частота сердечных сокращений (ЧСС)
        /// </summary>
        public short HeartRate { get; }

        /// <summary>
        /// Частотат дыхания (ЧД)
        /// </summary>
        public short RepsirationRate { get; }

        /// <summary>
        /// SPO2
        /// </summary>
        public short Spo2 { get; }
    }
}