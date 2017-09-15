using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Events.Control
{
    /// <summary>
    /// Событие выбрасывание исключения
    /// </summary>
    public class ExceptionThrownEvent : IEvent
    {
        public SessionProcessingException Exception { get; }
        
        public ExceptionThrownEvent(SessionProcessingException exception)
        {
            Exception = exception;
        }

    }
}