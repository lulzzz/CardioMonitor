using System;
using CardioMonitor.Core.Models.Patients;

namespace CardioMonitor.Core.Models.Session
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
