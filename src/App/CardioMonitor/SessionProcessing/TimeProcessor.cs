using System;
using CardioMonitor.Infrastructure.Threading;
using CardioMonitor.SessionProcessing.Events.Control;
using Enexure.MicroBus;
using JetBrains.Annotations;

namespace CardioMonitor.SessionProcessing
{
    /// <summary>
    /// Обработчик времени цикла
    /// </summary>
    public class TimeProcessor
    {
        private readonly IMicroBus _bus;

        [CanBeNull]
        private CardioTimer _timer;

        private TimeSpan _iterationTime;
        public TimeSpan CycleTime { get; private set; }

        public TimeSpan ElapsedTime { get; private set; }

        public TimeSpan RemainingTime => CycleTime - ElapsedTime;

        public TimeProcessor([NotNull] IMicroBus bus)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));

            _bus = bus;
        }
        
        public void Init(TimeSpan cycleDuration, TimeSpan cycleIteration)
        {
            _timer?.Stop();
            _timer = new CardioTimer(TimerTick, cycleDuration, cycleIteration);
            CycleTime = cycleDuration;
            _iterationTime = cycleIteration;
        }

        private async void TimerTick(object sender, EventArgs args)
        {
            await _bus.PublishAsync(new TimeUpdatedEvent(CycleTime, ElapsedTime)).ConfigureAwait(false);
            ElapsedTime += _iterationTime;
        }

        public void Start()
        {
            if (_timer == null) throw new InvalidOperationException("Timer not initialised");
            _timer.Start();
            ElapsedTime = TimeSpan.Zero;
        }

        public void Stop()
        {
            if (_timer == null) throw new InvalidOperationException("Timer not initialised");
            _timer.Stop();
        }

        public void Puase()
        {
            if (_timer == null) throw new InvalidOperationException("Timer not initialised");
            _timer.Suspend();
        }

        public void Resume()
        {
            if (_timer == null) throw new InvalidOperationException("Timer not initialised");

            _timer.Resume();
        }

    }
}