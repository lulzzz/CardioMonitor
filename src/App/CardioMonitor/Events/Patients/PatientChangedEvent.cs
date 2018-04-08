using Enexure.MicroBus;

namespace CardioMonitor.Events.Patients
{
    public class PatientChangedEvent : IEvent
    {
        public PatientChangedEvent(int patientId)
        {
            PatientId = patientId;
        }

        public int PatientId { get; }
    }
}