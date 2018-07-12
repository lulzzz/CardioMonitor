namespace CardioMonitor.FileSaving.Containers.V1
{
    internal class StoredPatientParamsV1
    { 
        /// <summary>
        /// Идентификатор
        /// </summary>
        public long Id { get; set; }

        public int SessionCycleId { get; set; }

        /// <summary>
        /// Специальное поле для корректной сортировки этих параметров
        /// </summary>
        public int Iteraton { get; set; }

        /// <summary>
        /// Угол наклона кровати
        /// </summary>
        public float InclinationAngle { get; set; }

        /// <summary>
        /// Частота сердечных сокращений (ЧСС)
        /// </summary>
        public StoredDeviceValueV1<short> HeartRate { get; set; }

        /// <summary>
        /// Частотат дыхания (ЧД)
        /// </summary>
        public StoredDeviceValueV1<short> RepsirationRate { get; set; }

        /// <summary>
        /// SPO2
        /// </summary>
        public StoredDeviceValueV1<short> Spo2 { get; set; }

        /// <summary>
        /// Систолическое артериальное давление
        /// </summary>
        public StoredDeviceValueV1<short> SystolicArterialPressure { get; set; }

        /// <summary>
        /// Диастолическое артериальное давление
        /// </summary>
        public StoredDeviceValueV1<short> DiastolicArterialPressure { get; set; }

        /// <summary>
        /// Среднее артериальное давлние 
        /// </summary>
        public StoredDeviceValueV1<short> AverageArterialPressure { get; set; }
    }
}
