

namespace CardioMonitor.Events.Sessions
{
    public class SessionDeletedEvent 
    {
        public SessionDeletedEvent(int sessionId)
        {
            SessionId = sessionId;
        }

        public int SessionId { get; }
    }
}