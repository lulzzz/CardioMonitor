using System;

namespace CardioMonitor.Devices.WpfModule
{
    public class WpfDeviceModule
    {
        public WpfDeviceModule(
            Guid deviceId, 
            Guid deviceTypeId, 
            string deviceName, 
            Type deviceControllerType, 
            Type deviceControllerConfigBuilder, 
            Type deviceControllerConfigViewModel)
        {
            if (String.IsNullOrWhiteSpace(deviceName)) throw new ArgumentNullException(nameof(deviceName));

            DeviceId = deviceId;
            DeviceTypeId = deviceTypeId;
            DeviceName = deviceName;
            DeviceControllerType = deviceControllerType ?? throw new ArgumentNullException(nameof(deviceControllerType));
            DeviceControllerConfigBuilder = deviceControllerConfigBuilder ?? throw new ArgumentNullException(nameof(deviceControllerConfigBuilder));
            DeviceControllerConfigViewModel = deviceControllerConfigViewModel ?? throw new ArgumentNullException(nameof(deviceControllerConfigViewModel));
        }

        public Guid DeviceId { get; }
        public Guid DeviceTypeId { get; }

        public string DeviceName { get; }

        public Type DeviceControllerType { get; }

        public Type DeviceControllerConfigBuilder { get; }

        public Type DeviceControllerConfigViewModel { get; }
    }

    public class DeviceTypeModule
    {
        public DeviceTypeModule(string deviceTypeName, Guid deviceTypeId)
        {
            if (String.IsNullOrWhiteSpace(deviceTypeName)) throw new ArgumentNullException(nameof(deviceTypeName));

            DeviceTypeName = deviceTypeName;
            DeviceTypeId = deviceTypeId;
        }

        public string DeviceTypeName { get; }

        public Guid DeviceTypeId { get; }
    }
}