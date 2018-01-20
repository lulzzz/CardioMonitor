using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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

        public CycleProcessingSynchronizer(
            [NotNull] BroadcastBlock<CycleProcessingContext> pipelineStartBlock,
            [NotNull] IWorkerController workerController)
        {
            _workerController = workerController ?? throw new ArgumentNullException(nameof(workerController));
            _pipelineStartBlock = pipelineStartBlock ?? throw new ArgumentNullException(nameof(pipelineStartBlock));
            IsPaused = false;
            _isProcessing = false;
            _processingPeriod = null;
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
            
            await _pipelineStartBlock
                .SendAsync(context)
                .ConfigureAwait(false);
        }


        /// <summary>
        /// Остановливает контроллер
        /// </summary>
        public void Stop()
        {
            if (!_isProcessing) throw new InvalidOperationException($"Необходимо сначала запустить контроллер методом {nameof(Start)}");
                
            IsPaused = false;
            _isProcessing = false;
            _workerController.CloseWorker(_worker);
        }

        /// <summary>
        /// Приостанавливает контроллер
        /// </summary>
        public void Pause()
        {
            if (_worker == null) throw new InvalidOperationException($"Необходимо сначала запустить обработку методом {nameof(Start)}");
            
            _worker.Stop();
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