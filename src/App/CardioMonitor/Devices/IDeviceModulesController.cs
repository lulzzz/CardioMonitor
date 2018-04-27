using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CardioMonitor.Devices.WpfModule;

namespace CardioMonitor.Devices
{
    public interface IDeviceModulesController
    {
        void Register(DeviceTypeModule module);

        void RegisterDevice(WpfDeviceModule module);

        IDeviceControllerConfigViewModel GetViewModel(Guid deviceId);
        UserControl GetView(Guid deviceId);

        ICollection<DeviceTypeInfo> GetRegisteredDevicesTypes();

        ICollection<DeviceInfo> GetRegisteredDevices(Guid deviceTypeId);
    }
}