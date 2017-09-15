using System;
using System.Threading.Tasks;
using CardioMonitor.Devices.Bed.Infrastructure;
using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing
{
    /// <summary>
    /// Модуль обработки сеанса
    /// </summary>
    /// <remarks>
    /// Точка входа по факту. Через нее все взаимодействие
    /// </remarks>
    public class SessionProcessingModule
    {
        private readonly IMicroBus _microBus;
        private readonly SessionProcessor _sessionProcessor;
        private readonly IBedController _bedController;
        private readonly TimeController _timeController;

        /// <summary>
        /// Длительность 1 тика таймера
        /// </summary>
        private readonly TimeSpan _cycleTick = TimeSpan.FromSeconds(1);
        
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

        #region Методы

        public async Task Start(CommandType commandType)
        {
            //todo сделать так, чтобы никто не мог вызвать повторно, если сеанс запущен или подготавливается
            
            // при старте мы должны узнать длительность сеанса, запустить обработку сеанса
            // накачка манжеты, измерение ЭКГ проводится внутри обработки сеанса

            var cycleDuration = await _bedController.GetCycleDurationAsync().ConfigureAwait(false);
            _timeController.Init(cycleDuration, _cycleTick);
            
            var cycleStateMachineBuilder = new CycleStateMachineBuilder();

            var cycleStateMachine = cycleStateMachineBuilder.Buid();
            
            _sessionProcessor.Start();
        }


        public void Suspend(CommandType commandType)
        {
            _sessionProcessor.Suspend();
        }

        public void Resume(CommandType commandType)
        {
            _sessionProcessor.Resume();
        }

        public void Reverse(CommandType commandType)
        {
            _sessionProcessor.Reverse();
        }

        public void EmergencyStop(CommandType commandType)
        {
            _sessionProcessor.EmergencyStop();
        }
        

        #endregion
        
    }
}