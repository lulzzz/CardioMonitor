using System.Collections.Generic;

namespace CardioMonitor.Data.Common.Entities.Sessions
{
    /// <summary>
    /// Цикл сеанса (повторение)
    /// </summary>
    public class SessionCycle
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        public int CycleNumber { get; set; }

        /// <summary>
        /// Показатели пациента за сенас
        /// </summary>
        public ICollection<PatientParams> PatientParams { get; set; }

        public int SessionId { get; set; }
        public virtual Session Session { get; set; }
    }
}