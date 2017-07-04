using System;
using System.Threading.Tasks;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.SessionProcessing.Events;
using CardioMonitor.SessionProcessing.Events.Devices;
using Enexure.MicroBus;
using JetBrains.Annotations;

namespace CardioMonitor.SessionProcessing.Handlers
{
    /// <summary>
    /// Менеджер взаимодействия с кроватью
    /// </summary>
    public class BedManager : IEventHandler<AngleRequestedEvent>
    {
        private readonly IBedController _bedController;
        private readonly IMicroBus _bus;

        public BedManager([NotNull] IBedController bedController, [NotNull] IMicroBus bus)
        {
            if (bedController == null) throw new ArgumentNullException(nameof(bedController));
            if (bus == null) throw new ArgumentNullException(nameof(bus));

            _bedController = bedController;
            _bus = bus;
        }

        public async Task Handle([NotNull] AngleRequestedEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var currentAngle = await _bedController.GetAngleXAsync();

            await _bus.PublishAsync(new AngleRecievedEvent {CurrentAngle = currentAngle});
        }
    }
}