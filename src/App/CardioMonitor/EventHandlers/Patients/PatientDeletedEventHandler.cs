using System;
using CardioMonitor.BLL.CoreContracts.Patients.Events;
using JetBrains.Annotations;
using Markeli.Utils.EventBus.Contracts;
using Markeli.Utils.EventBus.Local;

namespace CardioMonitor.EventHandlers.Patients
{
    public class PatientDeletedEventHandler : BaseLocalEventSubscriber<PatientDeletedEvent>
    {
        public event EventHandler<int> PatientDeleted;

        public PatientDeletedEventHandler(IEventBus eventBus) : base(eventBus)
        {
        }

        protected override void Consume([NotNull] PatientDeletedEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            PatientDeleted?.Invoke(this, @event.PatientId);
        }
    }
}