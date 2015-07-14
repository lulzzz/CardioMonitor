namespace CardioMonitor.Core.Repository.Monitor
{
    /// <summary>
    /// Параметры контрольной точки (угла)
    /// </summary>
    public class AngleCheckPoint
    {
        /// <summary>
        /// Признак подъема кровати
        /// </summary>
        public bool IsUppingPassed { get; set; }

        /// <summary>
        /// Признак спуска кровати
        /// </summary>
        public bool IsDowningPassed { get; set; }
    }
}
