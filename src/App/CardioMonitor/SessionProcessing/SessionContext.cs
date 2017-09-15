using System;
using CardioMonitor.BLL.CoreContracts.Session;

namespace CardioMonitor.SessionProcessing
{
    /// <summary>
    /// Контекст выполнения сеанса
    /// </summary>
    public class SessionContext
    {
        /// <summary>
        /// Агрегированный статус сеанса
        /// </summary>
        public SessionStatus SessionStatus { get; private set; }

        /// <summary>
        /// Длительность цикла
        /// </summary>
        public TimeSpan CycleTime { get; private set; }

        /// <summary>
        /// Прошедшее время
        /// </summary>
        public TimeSpan ElapsedTime { get; private set; }

        /// <summary>
        /// Оставшееся время
        /// </summary>
        public TimeSpan RemainingTime => CycleTime - ElapsedTime;
    }
}