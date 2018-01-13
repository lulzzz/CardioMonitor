using System;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines
{
    /// <summary>
    /// Параметры старта pipeline
    /// </summary>
    public class PipelineStartParams
    {
        public PipelineStartParams(TimeSpan cycleTickDuration, TimeSpan cycleDuration)
        {
            CycleTickDuration = cycleTickDuration;
            CycleDuration = cycleDuration;
        }

        /// <summary>
        /// Длительность тика таймера
        /// </summary>
        public TimeSpan CycleTickDuration;

        /// <summary>
        /// Длительность одного цикла
        /// </summary>
        public TimeSpan CycleDuration;
    }
}