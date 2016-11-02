using System;
using System.Collections.Generic;
using CardioMonitor.Data.Common.Entities.Patients;
using CardioMonitor.Data.Common.Entities.Sessions;

namespace CardioMonitor.Data.Common.Entities.Treatments
{
    /// <summary>
    /// Курс лечения
    /// </summary>
    public class Treatment
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор пациента
        /// </summary>
        public int PatientId { get; set; }

        public virtual Patient Patient { get; set; }

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
        public virtual ICollection<Session> Sessions { get; set; }
    }
}
