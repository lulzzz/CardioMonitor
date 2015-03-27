using System;

namespace CardioMonitor.Core.Models.Session
{
    /// <summary>
    /// Краткая информация о сеансе
    /// </summary>
    public class SessionInfo
    {
        /// <summary>
        /// Идентифкатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Дата и время сеанса
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Статус сеанса
        /// </summary>
        public SessionStatus Status { get; set; }
    }
}
