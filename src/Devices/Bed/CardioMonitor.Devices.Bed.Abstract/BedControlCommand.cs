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
        Start = 0,
        /// <summary>
        /// Остановка кровати
        /// </summary>
        Pause = 1,
        /// <summary>
        /// Обратное выполнение
        /// </summary>
        Reverse = 2, 
        /// <summary>
        /// Экстренная остановка
        /// </summary>
        EmergencyStop = 3,
        
        /// <summary>
        /// Выполнить калибровку (выравнивание кровати относительно горизонта)
        /// </summary>
        Callibrate = 4
    }
}
