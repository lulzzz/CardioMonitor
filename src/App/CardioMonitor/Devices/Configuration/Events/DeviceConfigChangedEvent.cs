using System;
using Markeli.Utils.EventBus.Contracts;

namespace CardioMonitor.Devices.Configuration.Events
{
    public class DeviceConfigChangedEvent : IEvent
    {
        public static readonly Guid EventTypeId = Guid.Parse("c1a0d3a8-fbb4-4bc4-8514-1c8ce73dc12c");

        public DeviceConfigChangedEvent(
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