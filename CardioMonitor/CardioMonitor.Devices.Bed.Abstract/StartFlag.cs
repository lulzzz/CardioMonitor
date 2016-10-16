namespace CardioMonitor.Devices.Bed.Infrastructure
{
    /// <summary>
    /// Флаг старт
    /// </summary>
    public enum StartFlag
    {
        /// <summary>
        /// Начальное состояние
        /// </summary>
        Default = -1,
        /// <summary>
        /// Пауза
        /// </summary>
        Pause = 0,
        /// <summary>
        /// Старт
        /// </summary>
        Start = 1
    }
}