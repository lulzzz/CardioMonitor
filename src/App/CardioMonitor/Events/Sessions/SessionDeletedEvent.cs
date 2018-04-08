using Enexure.MicroBus;

namespace CardioMonitor.Events.Patients
{
    public class SessionDeletedEvent : IEvent
    {
        public SessionDeletedEvent(int sessionId)
        {
            SessionId = sessionId;
        }

        public int SessionId { get; }
    }
}