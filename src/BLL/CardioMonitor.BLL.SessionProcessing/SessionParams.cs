namespace CardioMonitor.BLL.SessionProcessing
{
    /// <summary>
    /// Параметры сеанса
    /// </summary>
    public class SessionParams
    {
        /// <summary>
        /// Максимальный угол наклона кровати
        /// </summary>
        public float MaxAngle { get; }
        
        /// <summary>
        /// Количество повторений (циклов)
        /// </summary>
        public short CycleCount { get; }
        
        /// <summary>
        /// Частота
        /// </summary>
        public float Frequency { get; }

        public SessionParams(
            float maxAngle, 
            short cycleCount, 
            float frequency)
        {
            MaxAngle = maxAngle;
            CycleCount = cycleCount;
            Frequency = frequency;
        }
    }
}