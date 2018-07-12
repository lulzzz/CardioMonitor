using System;
using Markeli.Utils.EventBus.Contracts;

namespace CardioMonitor.Devices.Configuration.Events
{
    public class DeviceConfigAddedEvent : IEvent
    {
        public static readonly Guid EventTypeId = Guid.Parse("5b65f82a-d878-40d7-934d-ae7fa0900d0b");

        public DeviceConfigAddedEvent(
            Guid deviceConfigId)
        {
            DeviceConfigId = deviceConfigId;
            Id = Guid.NewGuid();
        }
        
        public Guid Id { get; }

        public Guid TypeId { get; } = EventTypeId;

        public Guid DeviceConfigId { get; }
    }
}