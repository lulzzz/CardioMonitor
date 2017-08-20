using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Events.Cycle
{
    public class CycleStoppedEvent : IEvent
    {
        public CycleStoppedEvent(bool hasErrors)
        {
            HasErrors = hasErrors;
        }

        public bool HasErrors { get; }
    }
}