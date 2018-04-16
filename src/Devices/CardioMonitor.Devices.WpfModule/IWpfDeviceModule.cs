using System;

namespace CardioMonitor.Devices.WpfModule
{
    public interface IWpfDeviceModule
    {
        Guid DeviceId { get; }
        Guid DeviceTypeId { get; }

        string DeviceName { get; }

        Type DeviceControllerType { get; }

        Type DeviceControllerConfigBuilder { get; }

        Type DeviceControllerConfigViewModel { get; }
    }
}