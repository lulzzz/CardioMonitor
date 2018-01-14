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
        public MaxAngle MaxAngle { get; }
        
        /// <summary>
        /// Количество повторений (циклов)
        /// </summary>
        public short CycleCount { get; }
        
        /// <summary>
        /// Частота
        /// </summary>
        public double Frequency { get; }

        public SessionParams(MaxAngle maxAngle, short cycleCount, double frequency)
        {
            MaxAngle = maxAngle;
            CycleCount = cycleCount;
            Frequency = frequency;
        }
    }
}