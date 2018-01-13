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
        public MaxAngle MaxAngle { get; private set; }
        
        /// <summary>
        /// Количество повторений (циклов)
        /// </summary>
        public short CycleCount { get; private set; }
        
        /// <summary>
        /// Частота
        /// </summary>
        public double Frequency { get; private set; }

        public SessionParams(MaxAngle maxAngle, short cycleCount, double frequency)
        {
            MaxAngle = maxAngle;
            CycleCount = cycleCount;
            Frequency = frequency;
        }
    }
}