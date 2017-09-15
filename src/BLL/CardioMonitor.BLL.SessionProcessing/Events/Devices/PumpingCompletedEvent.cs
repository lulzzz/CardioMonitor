using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Events.Devices
{
    /// <summary>
    /// Событие, возникающие при завершении накачки манжеты
    /// </summary>
    internal class PumpingCompletedEvent : IEvent
    {
        /// <summary>
        /// Признак успешной накачки
        /// </summary>
        public bool IsSuccessfully { get; set; }
    }
}