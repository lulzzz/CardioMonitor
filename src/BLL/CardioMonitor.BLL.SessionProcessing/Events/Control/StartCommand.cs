using CardioMonitor.SessionProcessing.Events.Control;
using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Events.Session
{
    /// <inheritdoc />
    /// <summary>
    /// Команда начала сеансы
    /// </summary>
    public class StartCommand : IControlCommand
    {
        public bool IsInternal { get; set; }
    }
}