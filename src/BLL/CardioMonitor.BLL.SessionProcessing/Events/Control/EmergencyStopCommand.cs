using CardioMonitor.SessionProcessing.Events.Control;
using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Events.Session
{
    /// <inheritdoc />
    /// <summary>
    /// Команда экстренной остановки 
    /// </summary>
    public class EmergencyStopCommand : IControlCommand
    {
        public bool IsInternal { get; set; }
    }
}