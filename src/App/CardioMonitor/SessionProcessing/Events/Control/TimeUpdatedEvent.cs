using System;
using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Events.Control
{
    /// <summary>
    /// Событие обновления времени цикла
    /// </summary>
    public class TimeUpdatedEvent : IEvent
    {
        public TimeUpdatedEvent(TimeSpan cycleTime, TimeSpan elapsedTime)
        {
            CycleTime = cycleTime;
            ElapsedTime = elapsedTime;
        }

        /// <summary>
        /// Длительность цикла
        /// </summary>
        public TimeSpan CycleTime { get; }

        /// <summary>
        /// Прошедшее время
        /// </summary>
        public TimeSpan ElapsedTime { get; }

        /// <summary>
        /// Оставшееся время
        /// </summary>
        public TimeSpan RemainingTime => CycleTime - ElapsedTime;

    }
}