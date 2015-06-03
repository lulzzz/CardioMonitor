using System;

namespace CardioMonitor.Core.Models.Patients
{
    /// <summary>
    /// Пациент
    /// </summary>
    [Serializable]
    public class Patient
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
    }
}
