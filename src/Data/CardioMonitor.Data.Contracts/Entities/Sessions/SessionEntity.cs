﻿using System;
using System.Collections.Generic;
using CardioMonitor.Data.Contracts.Entities.Patients;

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
        /// Идентификатор пациента
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// Пациент
        /// </summary>
        public virtual PatientEntity PatientEntity { get; set; }
        
        /// <summary>
        /// Циклы сеанса
        /// </summary>
        public virtual ICollection<SessionCycleEntity> Cycles { get; set; }
    }
}
