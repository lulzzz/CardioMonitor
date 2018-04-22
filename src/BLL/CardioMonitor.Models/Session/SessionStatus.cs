﻿namespace CardioMonitor.BLL.CoreContracts.Session
{
    /// <summary>
    /// Статус сеанса
    /// </summary>
    public enum SessionStatus
    {
        /// <summary>
        /// Неизвестно
        /// </summary>
        NotStarted,
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
        /// Запущен реверс
        /// </summary>
        Reverse,
        /// <summary>
        /// Экстренно остановлен
        /// </summary>
        EmergencyStopped
    }
}
