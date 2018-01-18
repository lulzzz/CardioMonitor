﻿using System;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    /// <summary>
    /// Параметры старта pipeline
    /// </summary>
    internal class PipelineStartParams
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