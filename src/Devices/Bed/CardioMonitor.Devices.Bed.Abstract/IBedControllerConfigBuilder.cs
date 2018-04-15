namespace CardioMonitor.Devices.Bed.Infrastructure
{
    public interface IBedControllerConfigBuilder : IDeviceControllerConfigBuilder
    {
        IBedControllerConfig Build(
            float maxAngleX,
            short cyclesCount,
            float movementFrequency,
            string jsonConfig);
    }
}