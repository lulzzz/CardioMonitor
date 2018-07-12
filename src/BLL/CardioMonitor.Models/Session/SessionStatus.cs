namespace CardioMonitor.BLL.CoreContracts.Session
{
    /// <summary>
    /// Статус сеанса
    /// </summary>
    public enum SessionStatus
    {
        /// <summary>
        /// Не начат
        /// </summary>
        NotStarted = 0,
        /// <summary>
        /// Заврешен
        /// </summary>
        Completed = 1,
        /// <summary>
        /// Прерван
        /// </summary>
        TerminatedOnError = 2,
        /// <summary>
        /// В процессе
        /// </summary>
        InProgress = 3,
        /// <summary>
        /// Приостановлен
        /// </summary>
        Suspended = 4,
        /// <summary>
        /// Экстренно остановлен
        /// </summary>
        EmergencyStopped = 5
    }
}
