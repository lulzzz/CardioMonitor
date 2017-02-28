using System;
using System.Collections.Generic;
using CardioMonitor.Data.Contracts.Entities.Treatments;

namespace CardioMonitor.Data.Contracts.Entities.Sessions
{
    /// <summary>
    /// Сеанс
    /// </summary>
    public class SessionEntity
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
        /// Идентификатор курса лечения
        /// </summary>
        public int TreatmentId { get; set; }

        /// <summary>
        /// Курс лечения
        /// </summary>
        public virtual TreatmentEntity TreatmentEntity { get; set; }
        
        /// <summary>
        /// Циклы сеанса
        /// </summary>
        public virtual ICollection<SessionCycleEntity> Cycles { get; set; }
    }
}
