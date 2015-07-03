namespace CardioMonitor.Core.Models.Connection
{
    /// <summary>
    /// Текущий статус кровати
    /// </summary>
    public enum BedMovingStatus
    {
        /// <summary>
        /// Готовоность к старту
        /// </summary>
        Ready,
        /// <summary>
        /// Подготовка к движению вверх
        /// </summary>
        PrepareToUpMoving,
        /// <summary>
        /// Движение наверх
        /// </summary>
        UpMoving,
        /// <summary>
        /// Подготовка к движению вниз
        /// </summary>
        PrepareToDownMoving,
        /// <summary>
        /// Движение вниз
        /// </summary>
        DownMoving,
        /// <summary>
        /// Ожидание после аварийной остановки
        /// </summary>
        WaitingAfterEmergency,
        /// <summary>
        /// Нет соединения
        /// </summary>
        UnConnected
    }

}
