namespace CardioMonitor.Data.Contracts.Entities.Sessions
{
    /// <summary>
    /// Показатели пациента
    /// </summary>
    public class PatientParamsEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Специальное поле для корректной сортировки этих параметров
        /// </summary>
        public int Iteration { get; set; }
        
        /// <summary>
        /// Угол наклона кровати
        /// </summary>
        public float InclinationAngle { get; set; }

        /// <summary>
        /// Частота сердечных сокращений (ЧСС)
        /// </summary>
        public short HeartRate { get; set; }

        /// <summary>
        /// Частота дыхания (ЧД)
        /// </summary>
        public short RepsirationRate { get; set; }

        /// <summary>
        /// SPO2
        /// </summary>
        public short Spo2 { get; set; }

        /// <summary>
        /// Систолическое артериальное давление
        /// </summary>
        public short SystolicArterialPressure { get; set; }

        /// <summary>
        /// Диастолическое артериальное давление
        /// </summary>
        public short DiastolicArterialPressure { get; set; }

        /// <summary>
        /// Среднее артериальное давлние 
        /// </summary>
        public short AverageArterialPressure { get; set; }

        public int SessionCycleId { get; set; }

        public virtual SessionCycleEntity SessionCycleEntity { get; set; }
    }
}
