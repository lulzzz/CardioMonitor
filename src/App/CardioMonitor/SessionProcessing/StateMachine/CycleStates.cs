using System;
using CardioMonitor.BLL.CoreContracts.Session;
using JetBrains.Annotations;
using Stateless;

namespace CardioMonitor.SessionProcessing.StateMachine
{
    public enum CycleStates
    {
        NotStarted,
        Prepared,
        InProgress,
        Suspedned,
        EmergencyStopped,
        Completed
    }

    public enum CycleTriggers
    {
        Start,
        PreparingCompleted,
        Suspend,
        Resume,
        EmergencyStop,
        Complete
    }

    //todo потенциально сделать ViewModel для уведомления
    public class SessionContext
    {
        /// <summary>
        /// Агрегированный статус сеанса
        /// </summary>
        public SessionStatus SessionStatus { get; private set; }

        /// <summary>
        /// Длительность цикла
        /// </summary>
        public TimeSpan CycleTime { get; private set; }

        /// <summary>
        /// Прошедшее время
        /// </summary>
        public TimeSpan ElapsedTime { get; private set; }

        /// <summary>
        /// Оставшееся время
        /// </summary>
        public TimeSpan RemainingTime => CycleTime - ElapsedTime;
    }

    public class CycleStateMachine
    {
        private readonly StateMachine<CycleStates, CycleTriggers> _stateMachine;


        public CycleStateMachine(
            StateMachine<CycleStates, CycleTriggers> stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Fire(CycleTriggers trigger)
        {
            _stateMachine.Fire(trigger);
        }
    }

    public class CycleStateMachineBuilder
    {
        private Action<SessionContext> _onPreparedAction;
        private Action<SessionContext> _onInPorgressAction;
        private Action<SessionContext> _onSuspendAction;
        private Action<SessionContext> _onEmergencyStopedAction;
        private Action<SessionContext> _onCompletedAction;

        private SessionContext _sessionContext;

        public CycleStateMachineBuilder SetOnPreparedAction(Action<SessionContext> action)
        {
            _onPreparedAction = action;
            return this;
        }

        public CycleStateMachineBuilder SetOnInPorgressAction(Action<SessionContext> action)
        {
            _onInPorgressAction = action;
            return this;
        }

        public CycleStateMachineBuilder SetContext([NotNull] SessionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            _sessionContext = context;
            return this;
        }

        public void Buid()
        {
            var stateMachine = new StateMachine<CycleStates, CycleTriggers>(CycleStates.NotStarted);

            stateMachine
                .Configure(CycleStates.NotStarted)
                .Permit(CycleTriggers.Start, CycleStates.Prepared);

            stateMachine
                .Configure(CycleStates.Prepared)
                .Permit(CycleTriggers.PreparingCompleted, CycleStates.InProgress)
                .Permit(CycleTriggers.EmergencyStop, CycleStates.EmergencyStopped)
                .OnEntry(() =>
                {
                    _onPreparedAction?.Invoke(_sessionContext);
                });

            stateMachine
                .Configure(CycleStates.InProgress)
                .Permit(CycleTriggers.Suspend, CycleStates.Suspedned)
                .Permit(CycleTriggers.Complete, CycleStates.Completed)
                .Permit(CycleTriggers.EmergencyStop, CycleStates.EmergencyStopped)
                .OnEntry(() =>
                {
                    _onInPorgressAction.Invoke(_sessionContext);
                });

            stateMachine
                .Configure(CycleStates.Suspedned)
                .Permit(CycleTriggers.Resume, CycleStates.InProgress)
                .Permit(CycleTriggers.EmergencyStop, CycleStates.EmergencyStopped)
                .OnEntry(() =>
                {
                    _onSuspendAction.Invoke(_sessionContext);
                });

            stateMachine
                .Configure(CycleStates.EmergencyStopped)
                .OnEntry(() =>
                {
                    _onSuspendAction.Invoke(_sessionContext);
                });

            stateMachine
                .Configure(CycleStates.Completed)
                .Permit(CycleTriggers.Start, CycleStates.Prepared)
                .OnEntry(() =>
                {
                    _onCompletedAction.Invoke(_sessionContext);
                });

        }
    }
}