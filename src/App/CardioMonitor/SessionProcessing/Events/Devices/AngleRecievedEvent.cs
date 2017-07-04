using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Events.Devices
{
    /// <summary>
    /// Событие получение значение текущего угла наклона кровати
    /// </summary>
    public class AngleRecievedEvent : IEvent
    {
        public double CurrentAngle { get; set; }
    }
}