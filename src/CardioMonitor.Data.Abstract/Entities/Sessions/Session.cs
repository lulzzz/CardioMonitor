using System;
using System.Collections.Generic;
using CardioMonitor.Data.Common.Entities.Treatments;

namespace CardioMonitor.Data.Common.Entities.Sessions
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
        /// Дата и время
        /// </summary>
        public DateTime DateTime { get; set; }
        
        /// <summary>
        /// Статус завершения сеанса
        /// </summary>
        public SessionCompletionStatus Status { get; set; }
        
        /// <summary>
        /// Циклы сеанса
        /// </summary>
        public virtual ICollection<SessionCycle> Cycles { get; set; }

        /// <summary>
        /// Идентификатор курса лечения
        /// </summary>
        public int TreatmentId { get; set; }

        /// <summary>
        /// Курс лечения
        /// </summary>
        public virtual Treatment Treatment { get; set; }
    }
}
