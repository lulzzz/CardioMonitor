using System;
using CardioMonitor.Events.Sessions;
using Markeli.Utils.EventBus.Contracts;
using Markeli.Utils.EventBus.Local;

namespace CardioMonitor.EventHandlers.Sessions
{
    public class SessionAddedEventHandler : BaseLocalEventSubscriber<SessionAddedEvent>
    {
        public event EventHandler<int> SessionAdded;

        public SessionAddedEventHandler(IEventBus eventBus) : base(eventBus)
        {
        }

        protected override void Consume(SessionAddedEvent @event)
        {
            SessionAdded?.Invoke(this, @event.SessionId);
        }
    }
}