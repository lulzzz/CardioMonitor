using CardioMonitor.Devices.Bed.Fake.WpfModule;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Devices.WpfModule;

namespace CardioMonitor.Devices.Monitor.Fake.WpfModule
{
    public static class FakeMonitorControllerModule
    {
        public static readonly WpfDeviceModule Module = new WpfDeviceModule(
            FakeMonitorDeviceId.DeviceId,
            MonitorDeviceTypeId.DeviceTypeId,
            "Эмулятор кардиомонитора", 
            typeof(FakeMonitorController),
            typeof(FakeMonitorControllerConfigBuilder),
            typeof(FakeMonitorControllerConfigViewModel),
            typeof(FakeMonitorControllerConfigView));
    }
}