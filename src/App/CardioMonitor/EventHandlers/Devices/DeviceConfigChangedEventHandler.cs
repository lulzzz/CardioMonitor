using System;
using CardioMonitor.Devices.Configuration.Events;
using JetBrains.Annotations;
using Markeli.Utils.EventBus.Contracts;
using Markeli.Utils.EventBus.Local;

namespace CardioMonitor.EventHandlers.Devices
{
    public class DeviceConfigChangedEventHandler : BaseLocalEventSubscriber<DeviceConfigChangedEvent>
    {
        public EventHandler<Guid> DeviceConfigChanged;

        public DeviceConfigChangedEventHandler(IEventBus eventBus) : base(eventBus)
        {
        }

        protected override void Consume([NotNull] DeviceConfigChangedEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            DeviceConfigChanged?.Invoke(this, @event.ConfigId);
        }
    }
}