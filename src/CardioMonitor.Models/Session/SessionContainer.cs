using System;
using CardioMonitor.Models.Patients;

namespace CardioMonitor.Models.Session
{
    /// <summary>
    /// Контейнер для хранения сеанса в файле
    /// </summary>
    [Serializable]
    public class SessionContainer
    {
        /// <summary>
        /// Пациент
        /// </summary>
        public Patient Patient { get; set; }

        /// <summary>
        /// Сеанс
        /// </summary>
        public Session Session { get; set; }
    }
}
