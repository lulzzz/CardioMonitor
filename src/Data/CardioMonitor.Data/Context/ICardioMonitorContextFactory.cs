namespace CardioMonitor.Data.Ef.Context
{
    public interface ICardioMonitorContextFactory
    {
        CardioMonitorContext Create();
    }
}