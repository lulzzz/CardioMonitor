namespace CardioMonitor.Devices.Bed.Infrastructure
{
    /// <summary>
    /// Статус кровати
    /// </summary>
    public enum BedStatus
    {
        /// <summary>
        /// Состояние неизвестно
        /// </summary>
        Unknown = -1,
        
        /// <summary>
        /// Устройство готово 
        /// </summary>
        Ready = 0,
        
        /// <summary>
        /// Подготовка к запуску
        /// </summary>
        Preparing = 1, 
        
        /// <summary>
        ///  Сеанс запущен
        /// </summary>
        SessionStarted = 2,
        
        /// <summary>
        /// Блокировка при старте с ПК
        /// </summary>
        BlockedFromPC = 3,
        
        /// <summary>
        /// Ошибка на устройстве
        /// </summary>
        Error = 5,
        /// <summary>
        ///  Устройство не готово (ожидание завершения возвращения)
        /// </summary>
        NotReady = 6
    }
}
