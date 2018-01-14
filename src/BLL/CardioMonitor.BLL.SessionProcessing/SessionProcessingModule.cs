using System;
using System.Threading.Tasks;
using Autofac;
using CardioMonitor.BLL.SessionProcessing.CycleProcessing.Time;
using CardioMonitor.BLL.SessionProcessing.CycleStateMachine;
using CardioMonitor.BLL.SessionProcessing.Processing;
using CardioMonitor.Devices.Bed.Infrastructure;
using Enexure.MicroBus;
using Enexure.MicroBus.Autofac;
using Stateless;

namespace CardioMonitor.BLL.SessionProcessing
{
    public interface ISessionProcessingModule
    {
        /// <summary>
        /// Контектс сенаса
        /// </summary>
        /// <remarks>
        /// Содержит в себе все данные сеанса
        /// </remarks>
        SessionContext Context { get; }

        /// <summary>
        /// Параметры сеанса
        /// </summary>
        SessionParams Params { get; }

        /// <summary>
        /// Событие изменения времени сеанса
        /// </summary>
        event EventHandler TimeChanged;

        /// <summary>
        /// Событие изменения текущего угла наклона кровати
        /// </summary>
        event EventHandler AngleChanged;

        /// <summary>
        /// Событие обновления данных пациента 
        /// </summary>
        event EventHandler PatientParamsChanged;

        /// <summary>
        /// События завершения сеанса
        /// </summary>
        event EventHandler SessionCompleted;

        event EventHandler SessionInitializeStarted;
        event EventHandler SessionInitilizeCompleted;
        void Init();
        Task Start(CommandType commandType);
        void Suspend(CommandType commandType);
        void Resume(CommandType commandType);
        void Reverse(CommandType commandType);
        void EmergencyStop(CommandType commandType);
    }

    /// <summary>
    /// Модуль обработки сеанса
    /// </summary>
    /// <remarks>
    /// Точка входа по факту. Через нее все взаимодействие
    /// </remarks>
    public class SessionProcessingModule 
        : ISessionProcessingModule

    {
        private readonly SessionParams _sessionParams;
        private IMicroBus _bus;
        private readonly IBedController _bedController;
        private CycleProcessingSynchroniaztionController _cycleProcessingSynchroniaztionController;
        
        
        
        private StateMachine<CycleStates, CycleTriggers> _stateMachine;

        #region MyRegion

        /// <summary>
        /// Количество попыток накачать манжету при старте сеанса
        /// </summary>
        private const int PumpingRepeatsCountOnStart = 3;
        
        /// <summary>
        /// Длительность 1 тика таймера
        /// </summary>
        private readonly TimeSpan _cycleTick = TimeSpan.FromSeconds(1);

        #endregion
        
        
        #region Свойства
        
        /// <summary>
        /// Контектс сенаса
        /// </summary>
        /// <remarks>
        /// Содержит в себе все данные сеанса
        /// </remarks>
        public SessionContext Context { get; private set; }
        
        /// <summary>
        /// Параметры сеанса
        /// </summary>
        public SessionParams Params { get; private set; }

        #endregion
        
        #region События

        /// <summary>
        /// Событие изменения времени сеанса
        /// </summary>
        public event EventHandler TimeChanged;

        /// <summary>
        /// Событие изменения текущего угла наклона кровати
        /// </summary>
        public event EventHandler AngleChanged;

        /// <summary>
        /// Событие обновления данных пациента 
        /// </summary>
        public event EventHandler PatientParamsChanged;

        /// <summary>
        /// События завершения сеанса
        /// </summary>
        public event EventHandler SessionCompleted;

        public event EventHandler SessionInitializeStarted;

        public event EventHandler SessionInitilizeCompleted;
        
        #endregion

        public SessionProcessingModule(
            SessionParams sessionParams,
            IBedController bedController)
        {
            _sessionParams = sessionParams;
            _bedController = bedController;

        }

        public void Init()
        {
            
            var builder = new ContainerBuilder();
            var busBuilder = new BusBuilder();
            //todo init busBuilder
            
            builder.RegisterMicroBus(busBuilder, new BusSettings { HandlerSynchronization = Synchronization.Syncronous });

            var container = builder.Build();
            _bus = container.Resolve<IMicroBus>();
            
           // _timeController = new TimeController(_bus);
        }

        #region Методы

        public async Task Start(CommandType commandType)
        {
            //todo сделать так, чтобы никто не мог вызвать повторно, если сеанс запущен или подготавливается
            
            // при старте мы должны узнать длительность сеанса, запустить обработку сеанса

            var cycleDuration = await _bedController.GetCycleDurationAsync().ConfigureAwait(false);
            _cycleProcessingSynchroniaztionController.Init(cycleDuration, _cycleTick);
            
            var cycleStateMachineBuilder = new CycleStateMachineBuilder();
            cycleStateMachineBuilder.SetOnPreparedAction(PrepareCycle);
            
            _stateMachine = cycleStateMachineBuilder.Buid();
            
            
            _stateMachine.Fire(CycleTriggers.Start); 
        }

        private void PrepareCycle(SessionContext context)
        {
            // накачка манжеты, измерение ЭКГ проводится внутри обработки сеанса
            SessionInitializeStarted?.Invoke(this, EventArgs.Empty);
            //_bus.(new PumpingRequestedEvent(PumpingRepeatsCountOnStart));
        }

        
        
        private void CompleteCyclePreparation()
        {
             SessionInitilizeCompleted?.Invoke(this, EventArgs.Empty);
            _stateMachine.Fire(CycleTriggers.PreparingCompleted);
        }

        public void Suspend(CommandType commandType)
        {
            _stateMachine.Fire(CycleTriggers.Suspend);
        }

        public void Resume(CommandType commandType)
        {
            _stateMachine.Fire(CycleTriggers.Resume);
        }

        public void Reverse(CommandType commandType)
        {
          //  _bus.PublishAsync(new ReverseCommand());
        }

        public void EmergencyStop(CommandType commandType)
        {
            _stateMachine.Fire(CycleTriggers.EmergencyStop);
        }
        

        #endregion

    }
}