using System;
using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Events.Control
{
    /// <summary>
    /// Событие обновления времени цикла
    /// </summary>
    /// <remarks>
    /// Т.е. реакция на каждый тик таймера
    /// </remarks>
    internal class TimeUpdatedEvent : IEvent
    {
        public TimeUpdatedEvent(TimeSpan cycleDuration, TimeSpan elapsedTime)
        {
            CycleDuration = cycleDuration;
            ElapsedTime = elapsedTime;
        }

        /// <summary>
        /// Длительность цикла
        /// </summary>
        public TimeSpan CycleDuration { get; }

        /// <summary>
        /// Прошедшее время
        /// </summary>
        public TimeSpan ElapsedTime { get; }

        /// <summary>
        /// Оставшееся время
        /// </summary>
        public TimeSpan RemainingTime => CycleDuration - ElapsedTime;

    }
}