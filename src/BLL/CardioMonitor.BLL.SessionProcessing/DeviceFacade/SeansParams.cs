using System;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    /// <summary>
    /// Параметры старта pipeline
    /// </summary>
    internal class SeansParams
    {
        public SeansParams(
            TimeSpan cycleTickDuration, 
            TimeSpan cycleDuration, 
            short cyclesCount)
        {
            CycleTickDuration = cycleTickDuration;
            CycleDuration = cycleDuration;
            CyclesCount = cyclesCount;
        }

        /// <summary>
        /// Длительность тика таймера
        /// </summary>
        public TimeSpan CycleTickDuration { get; }

        /// <summary>
        /// Длительность одного цикла
        /// </summary>
        public TimeSpan CycleDuration { get; }
        
        public short CyclesCount { get; }
    }
}