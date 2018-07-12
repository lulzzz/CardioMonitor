using System;
using Markeli.Utils.EventBus.Contracts;

namespace CardioMonitor.BLL.CoreContracts.Patients.Events
{
    public class PatientChangedEvent : IEvent
    {
        public static readonly Guid EventTypeId = Guid.Parse("c05a9a0e-fd19-4fec-920f-6dfae695e566");

        public PatientChangedEvent(int patientId)
        {
            PatientId = patientId;
            Id = Guid.NewGuid();
            TypeId = EventTypeId;
        }

        public int PatientId { get; }
        public Guid Id { get; }
        public Guid TypeId { get; }
    }
}