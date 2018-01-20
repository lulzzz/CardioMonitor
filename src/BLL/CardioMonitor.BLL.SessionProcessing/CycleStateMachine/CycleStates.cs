﻿namespace CardioMonitor.BLL.SessionProcessing.CycleStateMachine
{
    /// <summary>
    /// Возможные состояния цикла
    /// </summary>
    internal enum CycleStates
    {
        /// <summary>
        /// Не начат
        /// </summary>
        NotStarted,
        /// <summary>
        /// Цикл выполняется
        /// </summary>
        InProgress,
        /// <summary>
        /// Приостановлен
        /// </summary>
        Suspedned,
        /// <summary>
        /// Была экстренная остановка
        /// </summary>
        EmergencyStopped,
        /// <summary>
        /// Завершен
        /// </summary>
        Completed
    }
}