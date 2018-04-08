

namespace CardioMonitor.Events.Sessions
{
    public class SessionAddedEvent  
    {
        public SessionAddedEvent(int sessionId)
        {
            SessionId = sessionId;
        }

        public int SessionId { get; }
    }
}