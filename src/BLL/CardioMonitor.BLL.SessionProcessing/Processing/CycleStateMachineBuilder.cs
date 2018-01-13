using System;
using CardioMonitor.BLL.SessionProcessing.CycleStateMachine;
using JetBrains.Annotations;
using Stateless;

namespace CardioMonitor.BLL.SessionProcessing.Processing
{
    /// <summary>
    /// Построитель машины состояния цикла
    /// </summary>
    internal class CycleStateMachineBuilder
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

        public StateMachine<CycleStates, CycleTriggers> Buid()
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
                .Permit(CycleTriggers.Reset, CycleStates.NotStarted)
                .OnEntry(() =>
                {
                    _onCompletedAction.Invoke(_sessionContext);
                });


            return stateMachine;
        }
    }
}