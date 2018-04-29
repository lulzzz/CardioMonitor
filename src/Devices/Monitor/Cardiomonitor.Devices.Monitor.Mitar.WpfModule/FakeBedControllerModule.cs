using CardioMonitor.Devices.Monitor;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Devices.WpfModule;

namespace Cardiomonitor.Devices.Monitor.Mitar.WpfModule
{
    public static class MitarMonitorControllerModule
    {
        public static readonly WpfDeviceModule Module = new WpfDeviceModule(
            MitarMonitorDeviceId.DeviceId,
            MonitorDeviceTypeId.DeviceTypeId,
            "Митар", 
            typeof(MitarMonitorController),
            typeof(MitarMonitorControllerConfigBuilder),
            typeof(MitarMonitorControllerConfigViewModel),
            typeof(MitarMonitorControllerConfigView));
    }
}