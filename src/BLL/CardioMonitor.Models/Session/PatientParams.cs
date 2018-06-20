namespace CardioMonitor.BLL.CoreContracts.Session
{
    /// <summary>
    /// Показатели пациента
    /// </summary>
    public class PatientParams
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Специальное поле для корректной сортировки этих параметров
        /// </summary>
        public int Iteraton { get; set; }

        /// <summary>
        /// Угол наклона кровати
        /// </summary>
        public DeviceValue<float> InclinationAngle { get; set; }

        /// <summary>
        /// Частота сердечных сокращений (ЧСС)
        /// </summary>
        public DeviceValue<short> HeartRate { get; set; }

        /// <summary>
        /// Частотат дыхания (ЧД)
        /// </summary>
        public DeviceValue<short> RepsirationRate { get; set; }

        /// <summary>
        /// SPO2
        /// </summary>
        public DeviceValue<short> Spo2 { get; set; }

        /// <summary>
        /// Систолическое артериальное давление
        /// </summary>
        public DeviceValue<short> SystolicArterialPressure { get; set; }

        /// <summary>
        /// Диастолическое артериальное давление
        /// </summary>
        public DeviceValue<short> DiastolicArterialPressure { get; set; }

        /// <summary>
        /// Среднее артериальное давлние 
        /// </summary>
        public DeviceValue<short> AverageArterialPressure { get; set; }
    }
}
