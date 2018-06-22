using System.Collections.Generic;

namespace CardioMonitor.Data.Ef.Entities.Sessions
{
    /// <summary>
    /// Цикл сеанса (повторение)
    /// </summary>
    public class SessionCycleEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        public int CycleNumber { get; set; }

        /// <summary>
        /// Показатели пациента за сенас
        /// </summary>
        public ICollection<PatientParamsEntity> PatientParams { get; set; }

        public int SessionId { get; set; }

        public virtual SessionEntity SessionEntity { get; set; }
    }
}