namespace CardioMonitor.Core.Repository.BedController
{
    /// <summary>
    /// Флаг реверса
    /// </summary>
    public enum ReverseFlag
    {
        /// <summary>
        /// Изначальное состояние
        /// </summary>
        Default = -1,
        /// <summary>
        /// Реверс не вызван
        /// </summary>
        NotReversed = 0,
        /// <summary>
        /// Реверс вызван
        /// </summary>
        Reversed = 1
    }
}
