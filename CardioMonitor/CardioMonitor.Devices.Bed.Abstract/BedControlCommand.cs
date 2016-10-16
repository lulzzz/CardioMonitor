namespace CardioMonitor.Devices.Bed.Infrastructure
{
    /// <summary>
    /// Управляющая команда кроватью
    /// </summary>
    public enum BedControlCommand
    {
        /// <summary>
        /// Запуск кровати 
        /// </summary>
        Start,
        /// <summary>
        /// Остановка кровати
        /// </summary>
        Pause,
        /// <summary>
        /// Обратное выполнение
        /// </summary>
        Reverse, 
        /// <summary>
        /// Экстренная остановка
        /// </summary>
        EmergencyStop
    }
}
