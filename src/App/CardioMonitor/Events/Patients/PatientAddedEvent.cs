using System;
using Markeli.Utils.EventBus.Contracts;

namespace CardioMonitor.Events.Patients
{
    public class PatientAddedEvent  : IEvent
    {
        public static readonly Guid EventTypeId = Guid.Parse("bf385190-0faf-42d2-9e41-fa9169e551fd");

        public PatientAddedEvent()
        {
            Id = Guid.NewGuid();
            TypeId = EventTypeId;
        }

        public Guid Id { get; }
        public Guid TypeId { get; }
    }
}