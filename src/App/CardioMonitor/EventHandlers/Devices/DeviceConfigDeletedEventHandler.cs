using System;
using CardioMonitor.Events.Devices;
using CardioMonitor.Events.Settings;
using JetBrains.Annotations;
using Markeli.Utils.EventBus.Contracts;
using Markeli.Utils.EventBus.Local;

namespace CardioMonitor.EventHandlers.Devices
{
    public class DeviceConfigDeletedEventHandler : BaseLocalEventSubscriber<DeviceConfigChangedEvent>
    {
        public EventHandler<Guid> DeviceConfigDeleted;

        public DeviceConfigDeletedEventHandler(IEventBus eventBus) : base(eventBus)
        {
        }

        protected override void Consume([NotNull] DeviceConfigChangedEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            DeviceConfigDeleted?.Invoke(this, @event.ConfigId);
        }
    }
}