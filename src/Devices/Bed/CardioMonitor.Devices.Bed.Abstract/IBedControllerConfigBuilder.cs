namespace CardioMonitor.Devices.Bed.Infrastructure
{
    public interface IBedControllerConfigBuilder : IDeviceControllerConfigBuilder
    {
        IBedControllerConfig Build(
            string jsonConfig,
            float maxAngleX = 0f,
            short cyclesCount = 0,
            float movementFrequency = 0f);


        string Build(IBedControllerConfig config);
    }
}