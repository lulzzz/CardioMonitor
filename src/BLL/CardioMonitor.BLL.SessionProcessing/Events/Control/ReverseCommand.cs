using CardioMonitor.SessionProcessing.Events.Session;

namespace CardioMonitor.SessionProcessing.Events.Control
{
    /// <inheritdoc />
    /// <summary>
    /// Реверс
    /// </summary>
    public class ReverseCommand : IControlCommand
    {
        public bool IsInternal { get; set; }
    }
}