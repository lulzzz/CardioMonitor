namespace CardioMonitor.Infrastructure.Models.Session
{
    /// <summary>
    /// Статус сеанса
    /// </summary>
    public enum SessionStatus
    {
        /// <summary>
        /// Неизвестно
        /// </summary>
        Unknown,
        /// <summary>
        /// Заврешен
        /// </summary>
        Completed,
        /// <summary>
        /// Прерван
        /// </summary>
        Terminated,
        /// <summary>
        /// В процессе
        /// </summary>
        InProgress,
        /// <summary>
        /// Приостановлен
        /// </summary>
        Suspended
    }
}
