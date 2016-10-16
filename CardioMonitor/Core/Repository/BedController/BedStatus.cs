namespace CardioMonitor.Core.Repository.BedController
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
        /// Идет калибровка 
        /// </summary>
        Calibrating,
        /// <summary>
        /// Устройство готово 
        /// </summary>
        Ready,
        /// <summary>
        ///  Идет цикл
        /// </summary>
        Loop,
        /// <summary>
        ///  Устройство не готово (ожидание завершения возвращения)
        /// </summary>
        NotReady
    }
}
