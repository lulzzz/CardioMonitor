using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Bed.UDP;
using CardioMonitor.Devices.WpfModule;

namespace CardioMonitor.Devices.Bed.Udp.WpfModule
{
    public static class UdpInversionTableControllerModule
    {
        public static readonly WpfDeviceModule Module = new WpfDeviceModule(
            InversionTableV2UdpDeviceId.DeviceId,
            InversionTableDeviceTypeId.DeviceTypeId,
            "Контроллер инверсионного стола V2", 
            typeof(BedUDPController),
            typeof(BedUdpControllerConfigBuilder),
            typeof(UdpInversionTableControllerConfigViewModel),
            typeof(UdpInversionTableControllerConfigView));
    }
}