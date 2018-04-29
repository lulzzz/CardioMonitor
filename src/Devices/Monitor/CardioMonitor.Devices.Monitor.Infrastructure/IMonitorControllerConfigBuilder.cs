namespace CardioMonitor.Devices.Monitor.Infrastructure
{
    public interface IMonitorControllerConfigBuilder : IDeviceControllerConfigBuilder
    {
        IMonitorControllerConfig Build(string jsonConfig);


        string Build(IMonitorControllerConfig config);
    }
}