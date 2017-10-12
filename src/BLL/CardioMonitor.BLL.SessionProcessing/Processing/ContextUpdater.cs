namespace CardioMonitor.SessionProcessing
{
    /// <summary>
    /// Агрегирует события и обновляет данные контекста
    /// </summary>
    public class ContextUpdater
    {
        public SessionContext Context { get; private set; }
    }
}