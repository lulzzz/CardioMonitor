using System;
using Markeli.Utils.EventBus.Contracts;

namespace CardioMonitor.BLL.CoreContracts.Session.Events
{
    public class SessionDeletedEvent : IEvent
    {
        public static readonly Guid EventTypeId = Guid.Parse("1194c4f4-535c-4d1b-8ac3-478b91768bfa");

        public SessionDeletedEvent(int sessionId)
        {
            SessionId = sessionId;
            Id = Guid.NewGuid();
        }

        public int SessionId { get; }
        public Guid Id { get; }
        public Guid TypeId { get; } = EventTypeId;
    }
}