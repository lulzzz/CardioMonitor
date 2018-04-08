using System;
using Markeli.Utils.EventBus.Contracts;

namespace CardioMonitor.Events.Patients
{
    public class PatientDeletedEvent : IEvent
    {
        public static readonly Guid EventTypeId = Guid.Parse("79ab96a4-5471-492d-b26d-7fc6beb491a0");
        public PatientDeletedEvent(int patientId)
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