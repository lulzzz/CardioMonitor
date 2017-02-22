using System;

namespace CardioMonitor.BLL.CoreContracts.Treatment
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

        /// <summary>
        /// Дата начала курса
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Дата последнего сеанса
        /// </summary>
        public DateTime LastSessionDate { get; set; }

        /// <summary>
        /// Количество сеансов
        /// </summary>
        public int SessionsCount { get; set; }

        /// <summary>
        /// Курс лечения
        /// </summary>
        public Treatment()
        {
            StartDate = new DateTime();
            LastSessionDate = new DateTime();
        }
    }
}
