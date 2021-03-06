﻿using System;

namespace CardioMonitor.BLL.CoreContracts.Patients
{
    /// <summary>
    /// Полное имя пациента
    /// </summary>
    [Serializable]
    public class PatientFullName
    {
        public int PatientId { get; set; }

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
        /// Полное имя пациента
        /// </summary>
        public string Name
        {
            get
            {
                return String.Format("{0} {1} {2}", LastName, FirstName, PatronymicName);
            }
        }

        /// <summary>
        /// Возвращает полное имя пациента
        /// </summary>
        /// <returns>Полное имя пациента</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
