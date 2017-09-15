using CardioMonitor.SessionProcessing.Events.Control;
using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Events.Session
{
    /// <inheritdoc />
    /// <summary>
    /// Команда приостановки сеанса
    /// </summary>
    public class SuspendCommand : IControlCommand
    {
        public bool IsInternal { get; set; }
    }
}