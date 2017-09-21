using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Events.Devices
{
    /// <summary>
    /// Запрос на накачку манжеты 
    /// </summary>
    internal class PumpingRequestedEvent : IEvent
    {
        /// <summary>
        /// Количество попыток накачки
        /// </summary>
        public int RepeatsCount { get; }
        
        public PumpingRequestedEvent(int repeatsCount)
        {
            RepeatsCount = repeatsCount;
        }     
    }
}