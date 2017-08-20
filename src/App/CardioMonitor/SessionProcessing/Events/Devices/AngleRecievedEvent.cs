using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Events.Devices
{
    public class AngleRecievedEvent : IEvent
    {
        public AngleRecievedEvent(double currentAngle)
        {
            CurrentAngle = currentAngle;
        }

        public double CurrentAngle { get; }
    }
}