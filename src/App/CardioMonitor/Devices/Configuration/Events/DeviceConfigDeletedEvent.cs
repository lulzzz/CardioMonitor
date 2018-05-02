using System;
using Markeli.Utils.EventBus.Contracts;

namespace CardioMonitor.Events.Devices
{
    public class DeviceConfigDeletedEvent : IEvent
    {
        public static readonly Guid EventTypeId = Guid.Parse("bd724b31-f33d-4b46-95f3-2797f6df7868");

        public DeviceConfigDeletedEvent(
            Guid configId)
        {
            ConfigId = configId;
            Id = Guid.NewGuid();
        }

        public Guid ConfigId { get; }
        public Guid Id { get; }
        public Guid TypeId { get; } = EventTypeId;
    }
}