namespace CardioMonitor.Data.Common.Entities.Sessions
{
    /// <summary>
    /// Статус завершения сеанса
    /// </summary>
    public enum SessionCompletionStatus
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
    }
}