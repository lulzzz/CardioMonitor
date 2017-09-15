using CardioMonitor.SessionProcessing.Events.Session;

namespace CardioMonitor.SessionProcessing.Events.Control
{
    /// <inheritdoc />
    /// <summary>
    /// Команда продолжения работы после паузы
    /// </summary>
    public class ResumeCommand : IControlCommand
    {
        public bool IsInternal { get; set; }
    }
}