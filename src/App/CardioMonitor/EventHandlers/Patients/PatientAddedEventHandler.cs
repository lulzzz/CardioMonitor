using System;
using CardioMonitor.BLL.CoreContracts.Patients.Events;
using JetBrains.Annotations;
using Markeli.Utils.EventBus.Contracts;
using Markeli.Utils.EventBus.Local;

namespace CardioMonitor.EventHandlers.Patients
{
    public class PatientAddedEventHandler : BaseLocalEventSubscriber<PatientAddedEvent>
    {
        public event EventHandler<int> PatientAdded; 

        public PatientAddedEventHandler(IEventBus eventBus) : base(eventBus)
        {
        }

        protected override void Consume([NotNull] PatientAddedEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            PatientAdded?.Invoke(this, @event.PatientId);
        }
    }
}