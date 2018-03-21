namespace CardioMonitor.BLL.CoreContracts.Session
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
        TerminatedOnError,
        /// <summary>
        /// В процессе
        /// </summary>
        InProgress,
        /// <summary>
        /// Приостановлен
        /// </summary>
        Suspended,
        
        /// <summary>
        /// Экстренно остановлен
        /// </summary>
        EmergencyStopped,
    }
}
