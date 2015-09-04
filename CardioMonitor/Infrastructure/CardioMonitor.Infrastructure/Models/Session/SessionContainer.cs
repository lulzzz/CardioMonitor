using System;
using CardioMonitor.Infrastructure.Models.Patients;

namespace CardioMonitor.Infrastructure.Models.Session
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
