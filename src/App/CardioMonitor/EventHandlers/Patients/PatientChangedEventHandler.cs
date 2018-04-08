using System;
using CardioMonitor.Events.Patients;
using JetBrains.Annotations;
using Markeli.Utils.EventBus.Contracts;
using Markeli.Utils.EventBus.Local;

namespace CardioMonitor.EventHandlers.Patients
{
    public class PatientChangedEventHandler : BaseLocalEventSubscriber<PatientChangedEvent>
    {
        public event EventHandler<int> PatientChanged;

        public PatientChangedEventHandler(IEventBus eventBus) : base(eventBus)
        {
        }

        protected override void Consume([NotNull] PatientChangedEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            PatientChanged?.Invoke(this, @event.PatientId);
        }
    }
}