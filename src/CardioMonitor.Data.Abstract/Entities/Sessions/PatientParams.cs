namespace CardioMonitor.Data.Common.Entities.Sessions
{
    /// <summary>
    /// Показатели пациента
    /// </summary>
    public class PatientParams
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Специальное поле для корректной сортировки этих параметров
        /// </summary>
        public int Iteration { get; set; }

        /// <summary>
        /// Идентификатор сеанса лечения
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        /// Угол наклона кровати
        /// </summary>
        public double InclinationAngle { get; set; }

        /// <summary>
        /// Частота сердечных сокращений (ЧСС)
        /// </summary>
        public int HeartRate { get; set; }

        /// <summary>
        /// Частотат дыхания (ЧД)
        /// </summary>
        public int RepsirationRate { get; set; }

        /// <summary>
        /// SPO2
        /// </summary>
        public int Spo2 { get; set; }

        /// <summary>
        /// Систолическое артериальное давление
        /// </summary>
        public int SystolicArterialPressure { get; set; }

        /// <summary>
        /// Диастолическое артериальное давление
        /// </summary>
        public int DiastolicArterialPressure { get; set; }

        /// <summary>
        /// Среднее артериальное давлние 
        /// </summary>
        public int AverageArterialPressure { get; set; }

        public int SessionCycleId { get; set; }

        public virtual SessionCycle SessionCycle { get; set; }
    }
}
