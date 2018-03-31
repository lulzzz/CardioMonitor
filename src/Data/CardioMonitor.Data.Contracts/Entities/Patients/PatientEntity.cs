﻿using System;
using System.Collections.Generic;
using CardioMonitor.Data.Contracts.Entities.Treatments;

namespace CardioMonitor.Data.Contracts.Entities.Patients
{
    /// <summary>
    /// Пациент
    /// </summary>
    public class PatientEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        public string PatronymicName { get; set; }

        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateTime? BirthDate { get; set; }

        public virtual ICollection<PatientEntity> Patients { get; set; }
    }
}
