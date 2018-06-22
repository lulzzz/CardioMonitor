using System;
using Markeli.Utils.EventBus.Contracts;

namespace CardioMonitor.BLL.CoreContracts.Session.Events
{
    public class SessionAddedEvent : IEvent
    {
        public static readonly Guid EventTypeId = Guid.Parse("03ecea23-01d4-4fff-9de8-ac6e6bc1f261");

        public SessionAddedEvent(
            int sessionId)
        {
            SessionId = sessionId;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }

        public Guid TypeId { get; } = EventTypeId;

        public int SessionId { get; }
    }
}