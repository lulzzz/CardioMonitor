namespace CardioMonitor.Devices.Monitor.Infrastructure
{
    public interface IMonitorControllerConfigBuilder
    {
        IMonitorControllerConfig Build(string jsonConfig);
    }
}