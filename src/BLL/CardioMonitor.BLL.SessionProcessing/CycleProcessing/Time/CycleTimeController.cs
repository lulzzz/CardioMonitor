using System;
using System.Threading.Tasks.Dataflow;
using CardioMonitor.Infrastructure.Threading;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.CycleProcessing.Time
{
    /// <summary>
    /// Обработчик времени цикла
    /// </summary>
    internal class CycleTimeController
    {
        [NotNull] private readonly BroadcastBlock<PipelineContext> _pipelineStartBlock;

        [CanBeNull]
        private CardioTimer _timer;

        /// <summary>
        /// Длительность тика таймера
        /// </summary>
        private TimeSpan _cycleTickDuration;

        /// <summary>
        /// Длительность одного цикла
        /// </summary>
        private TimeSpan _cycleDuration;

        /// <summary>
        /// Прошедшее время цикла
        /// </summary>
        private TimeSpan _elapsedTime;


        public CycleTimeController([NotNull] BroadcastBlock<PipelineContext> pipelineStartBlock)
        {
            _pipelineStartBlock = pipelineStartBlock ?? throw new ArgumentNullException(nameof(pipelineStartBlock));
            IsPaused = false;
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
            _cycleDuration = cycleDuration;
            _cycleTickDuration = cycleTick;
        }

        private async void TimerTick(object sender, EventArgs args)
        {
            _elapsedTime += _cycleTickDuration;
            var context = new PipelineContext();
            
            var timeParams = new TimeContextParamses(_cycleDuration, _elapsedTime);
            context.AddOrUpdate(timeParams);
            
            await _pipelineStartBlock
                .SendAsync(context)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Запускает контроллера
        /// </summary>
        public void Start()
        {
            if (_timer == null) throw new InvalidOperationException("Timer not initialised");
            _timer.Start();
            _elapsedTime = TimeSpan.Zero;
            IsPaused = false;
        }

        public bool IsPaused { get; private set; }

        /// <summary>
        /// Остановливает контроллер
        /// </summary>
        public void Stop()
        {
            if (_timer == null) throw new InvalidOperationException("Timer not initialised");
            _timer.Stop();
            IsPaused = false;
        }

        /// <summary>
        /// Приостанавливается контроллер
        /// </summary>
        public void Puase()
        {
            if (_timer == null) throw new InvalidOperationException("Timer not initialised");
            _timer.Suspend();
            IsPaused = true;
        }

        /// <summary>
        /// Возобновляет работу контроллера после паузы
        /// </summary>
        public void Resume()
        {
            if (_timer == null) throw new InvalidOperationException("Timer not initialised");

            _timer.Resume();
            IsPaused = false;
        }

    }
}