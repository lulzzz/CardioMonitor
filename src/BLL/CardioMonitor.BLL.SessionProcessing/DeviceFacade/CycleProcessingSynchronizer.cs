using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.PressureParams;
using CardioMonitor.Infrastructure.Workers;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    /// <summary>
    /// Синхронизатор процесса обработки цикла. 
    /// </summary>
    /// <remarks>
    /// По факту выступает в роле тактового генератора, чтобы запускать опрос устройств, сбор данных и уведомление подписчиков
    /// </remarks>
    internal class CycleProcessingSynchronizer : IDisposable
    {
        [NotNull] private readonly BroadcastBlock<CycleProcessingContext> _pipelineStartBlock;

        private TimeSpan? _processingPeriod;

        [NotNull] 
        private readonly IWorkerController _workerController;

        [CanBeNull]
        private Worker _worker;

        private bool _isProcessing;
        private bool _isAutoPumpingEnabled;

        private readonly short _pumpingNumberOfAttempts;
        
        public CycleProcessingSynchronizer(
            [NotNull] BroadcastBlock<CycleProcessingContext> pipelineStartBlock,
            [NotNull] IWorkerController workerController,
            bool isAutoPumpingEnabled,
            short pumpingNumberOfAttempts)
        {
            _isAutoPumpingEnabled = isAutoPumpingEnabled;
            _workerController = workerController ?? throw new ArgumentNullException(nameof(workerController));
            _pipelineStartBlock = pipelineStartBlock ?? throw new ArgumentNullException(nameof(pipelineStartBlock));
            IsPaused = false;
            _isProcessing = false;
            _processingPeriod = null;
            _pumpingNumberOfAttempts = pumpingNumberOfAttempts;
        }
        
        public bool IsPaused { get; private set; }
        
        /// <summary>
        /// Инициализирует контроллера
        /// </summary>
        /// <param name="processingPeriod">Периодичность запуска всего процесса обработки</param>
        public void Init(TimeSpan processingPeriod)
        {
            _processingPeriod = processingPeriod;
        }

        public void EnableAutoPumping()
        {
            _isAutoPumpingEnabled = true;
        }
        
        public void DisableAutoPumping()
        {
            _isAutoPumpingEnabled = false;
        }
        
        /// <summary>
        /// Запускает контроллера
        /// </summary>
        public void Start()
        {
            if (!_processingPeriod.HasValue) 
                throw new InvalidOperationException($"Необходимо сначала инициализировать контроллер методом {nameof(Init)}");
            _worker = _workerController.StartWorker(_processingPeriod.Value, async () => await SyncAsync().ConfigureAwait(false));
            _isProcessing = true;
            IsPaused = false;
        }
        
        private async Task SyncAsync()
        {
            var context = new CycleProcessingContext();
            
            context.AddOrUpdate(
                new PumpingRequestContextParams(
                    _isAutoPumpingEnabled, 
                    _pumpingNumberOfAttempts));
            
            await _pipelineStartBlock
                .SendAsync(context)
                .ConfigureAwait(false);
        }


        /// <summary>
        /// Остановливает контроллер
        /// </summary>
        public void Stop()
        {
            IsPaused = false;
            _isProcessing = false;
            _workerController.CloseWorker(_worker);
        }

        /// <summary>
        /// Приостанавливает контроллер
        /// </summary>
        public void Pause()
        {
            _worker?.Stop();
            IsPaused = true;
        }

        /// <summary>
        /// Возобновляет работу контроллера после паузы
        /// </summary>
        public void Resume()
        {
            if (_worker == null) throw new InvalidOperationException($"Необходимо сначала запустить обработку методом {nameof(Start)}");

            _worker.Start();
            IsPaused = false;
        }

        public void Dispose()
        {
            _workerController.CloseWorker(_worker);
        }
    }
}