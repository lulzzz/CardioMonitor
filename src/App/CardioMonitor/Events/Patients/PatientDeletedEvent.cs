using Enexure.MicroBus;

namespace CardioMonitor.Events.Patients
{
    public class PatientDeletedEvent : IEvent
    {
        public PatientDeletedEvent(int patientId)
        {
            PatientId = patientId;
        }

        public int PatientId { get; }
    }
}