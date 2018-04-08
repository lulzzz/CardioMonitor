using Enexure.MicroBus;

namespace CardioMonitor.Events.Patients
{
    public class PatientAddedEvent  : IEvent
    {
        public PatientAddedEvent(int patientId)
        {
            PatientId = patientId;
        }

        public int PatientId { get; }
    }

    public class PatientDeletedEvent : IEvent
    {
        public PatientDeletedEvent(int patientId)
        {
            PatientId = patientId;
        }

        public int PatientId { get; }
    }
}