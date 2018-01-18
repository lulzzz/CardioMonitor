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
        Unknown,
        /// <summary>
        /// Соединение не установлено или разорвано 
        /// </summary>
        Disconnected,
        /// <summary>
        /// Подготовка к запуску
        /// </summary>
        Preparing = 1, 
        /// <summary>
        /// Устройство готово 
        /// </summary>
        Ready = 0,
        /// <summary>
        ///  Идет цикл
        /// </summary>
        Loop = 2,
        /// <summary>
        /// Ошибка на устройстве
        /// </summary>
        Error = 5,
        /// <summary>
        /// Блокировка при старте с ПК
        /// </summary>
        Block = 3,
        /// <summary>
        ///  Устройство не готово (ожидание завершения возвращения)
        /// </summary>
        NotReady
    }
}
