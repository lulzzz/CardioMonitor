namespace CardioMonitor.BLL.SessionProcessing.CycleStateMachine
{
    /// <summary>
    /// Триггеры для перехода между состояниямми в StateMachine
    /// </summary>
    internal enum CycleTriggers
    {
        /// <summary>
        /// Была дана команда старта
        /// </summary>
        Start,
        /// <summary>
        /// Была дана команда подготовки цикла
        /// </summary>
        PreparingCompleted,
        /// <summary>
        /// Была дана команды приостановки цикла
        /// </summary>
        Suspend,
        /// <summary>
        /// Была дана команда продолжить цикл после паузы
        /// </summary>
        Resume,
        /// <summary>
        /// Была дана команда экстренной остановки
        /// </summary>
        EmergencyStop,
        /// <summary>
        /// Цикл завершился
        /// </summary>
        Complete,
        /// <summary>
        /// Возвращение StateMachine в исходное состояние
        /// </summary>
        Reset
    }
}