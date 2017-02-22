using System;
using System.Collections.Generic;
using CardioMonitor.Data.Contracts.Entities.Patients;
using CardioMonitor.Data.Contracts.Entities.Sessions;

namespace CardioMonitor.Data.Contracts.Entities.Treatments
{
    /// <summary>
    /// Курс лечения
    /// </summary>
    public class TreatmentEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор пациента
        /// </summary>
        public int PatientId { get; set; }

        public virtual PatientEntity PatientEntity { get; set; }

        /// <summary>
        /// Дата начала курса
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Дата последнего сеанса
        /// </summary>
        public DateTime? LastSessionDate { get; set; }

        /// <summary>
        /// Количество сеансов
        /// </summary>
        public int SessionsCount { get; set; }
        
        /// <summary>
        /// Сеансы лечения
        /// </summary>
        public virtual ICollection<SessionEntity> Sessions { get; set; }
    }
}
