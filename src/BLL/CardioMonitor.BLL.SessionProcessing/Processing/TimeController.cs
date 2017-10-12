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
    internal class TimeController
    {
        /// <summary>
        /// Шина событий 
        /// </summary>
        private readonly IMicroBus _bus;

        [CanBeNull]
        private CardioTimer _timer;

        /// <summary>
        /// Длительность тика таймера
        /// </summary>
        private TimeSpan _cycleTickDuration;

        /// <summary>
        /// Длительность одного цикла
        /// </summary>
        public TimeSpan CycleDuration { get; private set; }

        /// <summary>
        /// Прошедшее время цикла
        /// </summary>
        public TimeSpan ElapsedTime { get; private set; }

        /// <summary>
        /// Оставшееся время
        /// </summary>
        public TimeSpan RemainingTime => CycleDuration - ElapsedTime;

        public TimeController([NotNull] IMicroBus bus)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));

            _bus = bus;
        }
        
        /// <summary>
        /// Инициализирует контроллера
        /// </summary>
        /// <param name="cycleDuration">Длитнльность цикла</param>
        /// <param name="cycleTick">Длительность тика цикла</param>
        public void Init(TimeSpan cycleDuration, TimeSpan cycleTick)
        {
            _timer?.Stop();
            _timer = new CardioTimer(TimerTick, cycleDuration, cycleTick);
            CycleDuration = cycleDuration;
            _cycleTickDuration = cycleTick;
        }

        private async void TimerTick(object sender, EventArgs args)
        {
            await _bus.PublishAsync(new TimeUpdatedEvent(CycleDuration, ElapsedTime)).ConfigureAwait(false);
            ElapsedTime += _cycleTickDuration;
        }

        /// <summary>
        /// Запускает контроллера
        /// </summary>
        public void Start()
        {
            if (_timer == null) throw new InvalidOperationException("Timer not initialised");
            _timer.Start();
            ElapsedTime = TimeSpan.Zero;
        }

        /// <summary>
        /// Остановливает контроллер
        /// </summary>
        public void Stop()
        {
            if (_timer == null) throw new InvalidOperationException("Timer not initialised");
            _timer.Stop();
        }

        /// <summary>
        /// Приостанавливается контроллер
        /// </summary>
        public void Puase()
        {
            if (_timer == null) throw new InvalidOperationException("Timer not initialised");
            _timer.Suspend();
        }

        /// <summary>
        /// Возобновляет работу контроллера после паузы
        /// </summary>
        public void Resume()
        {
            if (_timer == null) throw new InvalidOperationException("Timer not initialised");

            _timer.Resume();
        }

    }
}