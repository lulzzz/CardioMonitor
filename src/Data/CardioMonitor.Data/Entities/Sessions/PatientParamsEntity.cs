using CardioMonitor.Data.Ef.Entities.Sessions;

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
        /// Статус получения укла наклона (<see cref="InclinationAngle"/>)
        /// </summary>
        public DaoDeviceValueStatus InclinationAngleStatus { get; set; }

        /// <summary>
        /// Частота сердечных сокращений (ЧСС)
        /// </summary>
        public short HeartRate { get; set; }

        /// <summary>
        /// Статус получения ЧСС (<see cref="HeartRate"/>)
        /// </summary>
        public DaoDeviceValueStatus HeartRateStatus { get; set; }
            
        /// <summary>
        /// Частота дыхания (ЧД)
        /// </summary>
        public short RepsirationRate { get; set; }

        /// <summary>
        /// Статус получения ЧД (<see cref="RepsirationRate"/>)
        /// </summary>
        public DaoDeviceValueStatus RepsirationRateStatus { get; set; }
            
        /// <summary>
        /// SPO2
        /// </summary>
        public short Spo2 { get; set; }

        /// <summary>
        /// Статус получения Spo2 (<see cref="Spo2"/>)
        /// </summary>
        public DaoDeviceValueStatus Spo2Status { get; set; }
            
        /// <summary>
        /// Систолическое артериальное давление
        /// </summary>
        public short SystolicArterialPressure { get; set; }

        /// <summary>
        /// Статус получения систолического артериального давления (<see cref="SystolicArterialPressure"/>)
        /// </summary>
        public DaoDeviceValueStatus SystolicArterialPressureStatus { get; set; }
            
        /// <summary>
        /// Диастолическое артериальное давление
        /// </summary>
        public short DiastolicArterialPressure { get; set; }

        /// <summary>
        /// Статус получения диастолического артериального давления (<see cref="DiastolicArterialPressure"/>)
        /// </summary>
        public DaoDeviceValueStatus DiastolicArterialPressureStatus { get; set; }
            
        /// <summary>
        /// Среднее артериальное давлние 
        /// </summary>
        public short AverageArterialPressure { get; set; }

        /// <summary>
        /// Статус получения среднего АД (<see cref="AverageArterialPressure"/>)
        /// </summary>
        public DaoDeviceValueStatus AverageArterialPressureStatus { get; set; }
            
        public int SessionCycleId { get; set; }

        public virtual SessionCycleEntity SessionCycleEntity { get; set; }
    }
}
