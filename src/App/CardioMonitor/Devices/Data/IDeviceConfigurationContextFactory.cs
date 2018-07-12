namespace CardioMonitor.Devices.Data
{
    internal interface IDeviceConfigurationContextFactory
    {
        DeviceConfigurationContext Create();
    }
}