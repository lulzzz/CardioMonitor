namespace CardioMonitor.Core.Models.Connection
{

    /// <summary>
    /// Состояние соединения с кроватью
    /// </summary>
    public enum BedConnectionStatus
    {
        /// <summary>
        /// Соединение не установлено или разорвано 
        /// </summary>
        UnConnected,
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
        NotReady,
        /// <summary>
        /// Состояние неизвестно
        /// </summary>
        Unknow
    }
}
