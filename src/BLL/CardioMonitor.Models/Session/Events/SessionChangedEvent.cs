using System;
using Markeli.Utils.EventBus.Contracts;

namespace CardioMonitor.Events.Sessions
{
    public class SessionChangedEvent : IEvent
    {
        public static readonly Guid EventTypeId = Guid.Parse("c980cd66-09e1-4802-9b0a-08c9f4050f90");

        public SessionChangedEvent(int sessionId)
        {
            SessionId = sessionId;
            Id = Guid.NewGuid();
        }

        public int SessionId { get; }
        public Guid Id { get; }
        public Guid TypeId { get; } = EventTypeId;
    }
}