using System;
using CardioMonitor.Devices.Configuration.Events;
using Markeli.Utils.EventBus.Contracts;
using Markeli.Utils.EventBus.Local;

namespace CardioMonitor.EventHandlers.Devices
{
    public class DeviceConfigAddedEventHandler : BaseLocalEventSubscriber<DeviceConfigAddedEvent>
    {
        public EventHandler<Guid> DeviceConfigAdded;

        public DeviceConfigAddedEventHandler(IEventBus eventBus) : base(eventBus)
        {
        }

        protected override void Consume(DeviceConfigAddedEvent @event)
        {
            DeviceConfigAdded?.Invoke(this, @event.DeviceConfigId);
        }
    }
}