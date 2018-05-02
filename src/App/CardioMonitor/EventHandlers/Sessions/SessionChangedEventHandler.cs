using System;
using CardioMonitor.Events.Sessions;
using JetBrains.Annotations;
using Markeli.Utils.EventBus.Contracts;
using Markeli.Utils.EventBus.Local;

namespace CardioMonitor.EventHandlers.Sessions
{
    public class SessionChangedEventHandler : BaseLocalEventSubscriber<SessionChangedEvent>
    {
        public event EventHandler<int> SessionChanged;

        public SessionChangedEventHandler(IEventBus eventBus) : base(eventBus)
        {
        }

        protected override void Consume([NotNull] SessionChangedEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            SessionChanged?.Invoke(this, @event.SessionId);
        }
    }
}