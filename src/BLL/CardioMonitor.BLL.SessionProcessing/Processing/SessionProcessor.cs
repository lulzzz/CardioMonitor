using CardioMonitor.SessionProcessing.CycleStateMachine;
using CardioMonitor.SessionProcessing.Events.Control;
using Enexure.MicroBus;
using Stateless;

namespace CardioMonitor.SessionProcessing
{
    /// <summary>
    /// Класс для управления всем процессом сеанса
    /// </summary>
    internal class SessionProcessor
    {
        private readonly IMicroBus _bus;
        
        private readonly TimeController _timeController;

        private readonly SessionParams _sessionParams;

        private readonly StateMachine<CycleStates, CycleTriggers> _stateMachine;
        
        public void Start()
        {
            _stateMachine.Fire(CycleTriggers.Start);   
        }

        public void Suspend()
        {
            _stateMachine.Fire(CycleTriggers.Suspend);
        }

        public void Resume()
        {
            _stateMachine.Fire(CycleTriggers.Reset);
        }

        public void Reverse()
        {
            _bus.PublishAsync(new ReverseCommand());
        }

        public void EmergencyStop()
        {
            _stateMachine.Fire(CycleTriggers.EmergencyStop);
        }
    }
}