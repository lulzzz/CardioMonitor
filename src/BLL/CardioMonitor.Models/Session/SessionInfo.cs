using System;

namespace CardioMonitor.BLL.CoreContracts.Session
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
        /// Дата и время сеанса (UTC)
        /// </summary>
        public DateTime TimestampUtc { get; set; }

        /// <summary>
        /// Статус сеанса
        /// </summary>
        public SessionStatus Status { get; set; }
    }
}
