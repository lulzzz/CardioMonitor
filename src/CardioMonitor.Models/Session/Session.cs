using System;

namespace CardioMonitor.Models.Session
{
    /// <summary>
    /// Сеанс
    /// </summary>
    [Serializable]
    public class Session
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор курса лечения
        /// </summary>
        public int TreatmentId { get; set; }

        /// <summary>
        /// Дата и время
        /// </summary>
        public DateTime DateTime { get; set; }
        
        /// <summary>
        /// Статус сеанса
        /// </summary>
        public SessionStatus Status { get; set; }

        /// <summary>
        /// Сеанс
        /// </summary>
        public Session()
        {
            DateTime = new DateTime();
            Status = SessionStatus.Unknown;
        }
    }
}
