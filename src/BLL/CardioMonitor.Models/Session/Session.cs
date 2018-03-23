using System;
using System.Collections.Generic;

namespace CardioMonitor.BLL.CoreContracts.Session
{
    /// <summary>
    /// Сеанс
    /// </summary>
    public class Session
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор курса лечения
        /// </summary>
        public int TreatmentId { get; set; }

        /// <summary>
        /// Дата и время
        /// </summary>
        public DateTime DateTime { get; set; }
        
        /// <summary>
        /// Статус сеанса
        /// </summary>
        public SessionStatus Status { get; set; }

        public List<SessionCycle> Cycles { get; set; }

        /// <summary>
        /// Сеанс
        /// </summary>
        public Session()
        {
            DateTime = new DateTime();
            Status = SessionStatus.NotStarted;
        }
    }
}
