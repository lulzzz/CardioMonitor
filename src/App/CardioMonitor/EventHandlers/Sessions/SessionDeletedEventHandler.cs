using System;
using CardioMonitor.Events.Sessions;
using JetBrains.Annotations;
using Markeli.Utils.EventBus.Contracts;
using Markeli.Utils.EventBus.Local;

namespace CardioMonitor.EventHandlers.Sessions
{
    public class SessionDeletedEventHandler : BaseLocalEventSubscriber<SessionDeletedEvent>
    {
        public event EventHandler<int> SessionDeleted;

        public SessionDeletedEventHandler(IEventBus eventBus) : base(eventBus)
        {
        }

        protected override void Consume([NotNull] SessionDeletedEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            SessionDeleted?.Invoke(this, @event.SessionId);
        }
    }
}