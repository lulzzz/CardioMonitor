using System;
using System.Collections.Generic;
using System.Windows;
using CardioMonitor.Devices.WpfModule;

namespace CardioMonitor.Devices
{
    public interface IDeviceModulesController
    {
        void Register(DeviceTypeModule module);

        void RegisterDevice(WpfDeviceModule module);

        IDeviceControllerConfigViewModel GetViewModel(Guid deviceId);
        UIElement GetView(Guid deviceId);

        ICollection<DeviceTypeInfo> GetRegisteredDevicesTypes();

        ICollection<DeviceInfo> GetRegisteredDevices(Guid deviceTypeId);
    }
}