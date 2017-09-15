using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Events.Devices
{
    /// <summary>
    /// События получения значения текущего угла наклона кровати
    /// </summary>
    internal class AngleRecievedEvent : IEvent
    {
        public AngleRecievedEvent(double currentAngle)
        {
            CurrentAngle = currentAngle;
        }

        public double CurrentAngle { get; }
    }
}