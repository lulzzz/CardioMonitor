using System;
using System.Threading.Tasks;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.SessionProcessing.Events.Control;
using CardioMonitor.SessionProcessing.Events.Cycle;
using CardioMonitor.SessionProcessing.Events.Devices;
using CardioMonitor.SessionProcessing.Resolvers;
using Enexure.MicroBus;
using JetBrains.Annotations;

namespace CardioMonitor.SessionProcessing
{
    /// <summary>
    /// Обработчик угла наклона кровати
    /// </summary>
    /// <remarks>
    /// Должен быть один на цикл
    /// </remarks>
    public class AngleProcessor : 
        IEventHandler<TimeUpdatedEvent>,
        IEventHandler<CycleReverseRequestedEvent>
    {
        private readonly IBedController _bedController;
        private readonly IMicroBus _bus;
        private readonly PumpingResolver _pumpingResolver;
        private readonly CheckPointResolver _checkPointResolver;

        private bool _isUpping = true;

        public AngleProcessor(
            [NotNull] IMicroBus bus, 
            [NotNull] PumpingResolver pumpingResolver,
            [NotNull] CheckPointResolver checkPointResolver,
            [NotNull] IBedController bedController )
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            if (pumpingResolver == null) throw new ArgumentNullException(nameof(pumpingResolver));
            if (checkPointResolver == null) throw new ArgumentNullException(nameof(checkPointResolver));

            if (bedController == null) throw new ArgumentNullException(nameof(bedController));
            if (bus == null) throw new ArgumentNullException(nameof(bus));

            _bus = bus;
            _bedController = bedController;
            _pumpingResolver = pumpingResolver;
            _checkPointResolver = checkPointResolver;
        }
        
        public async Task Handle(TimeUpdatedEvent @event)
        {
            if (@event.ElapsedTime >= @event.RemainingTime)
            {
                _isUpping = false;
            }

            var currentAngle = await _bedController.GetAngleXAsync();
            var needPumping = _pumpingResolver.NeedPumping(currentAngle, _isUpping);
            if (needPumping)
            {
                await _bus.PublishAsync(new PumpingRequestedEvent());
            }

            var isCheckPoint = _checkPointResolver.IsNeedUpdateData(currentAngle);
            if (isCheckPoint)
            {
                await _bus.PublishAsync(new CheckPointReachedEvent());
            }

            await _bus.PublishAsync(new AngleRecievedEvent(currentAngle));
        }

        public async Task Handle(CycleReverseRequestedEvent @event)
        {
            await Task.Yield();
            _checkPointResolver.ConsiderReversing();
        }
    }
}