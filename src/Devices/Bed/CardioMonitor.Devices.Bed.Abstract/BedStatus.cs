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
        ///  Сеанс запущен
        /// </summary>
        SessionStarted = 1,
        
        /// <summary>
        /// Экстренная остановка
        /// </summary>
        EmergencyStop = 2,
        
        /// <summary>
        /// Запущен реверс
        /// </summary>
        Reverse = 3,
        
        /// <summary>
        /// Пауза
        /// </summary>
        Pause = 4,
        
        /// <summary>
        /// Блокировка при старте с ПК
        /// </summary>
        BlockedFromPC = 5,
        
        /// <summary>
        /// Ошибка на устройстве
        /// </summary>
        Error = 6,
        /// <summary>
        ///  Устройство не готово (ожидание завершения возвращения)
        /// </summary>
        NotReady = 7
    }
}
