using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.WpfModule;

namespace CardioMonitor.Devices.Bed.Fake.WpfModule
{
    public static class FakeBedControllerModule
    {
        public static readonly WpfDeviceModule Module = new WpfDeviceModule(
            FakeInversionTableDeviceId.DeviceId,
            InversionTableDeviceTypeId.DeviceTypeId,
            "Эмулятор кровати", 
            typeof(FakeBedController),
            typeof(FakeBedControllerConfigBuilder),
            typeof(FakeBedControllerConfigViewModel),
            typeof(FakeBedControllerConfigView));
    }
}