using System;

namespace CardioMonitor.Devices.Configuration
{
    public class DeviceConfiguration
    {
        public Guid ConfigId { get; }

        public string ConfigName { get; }

        public Guid DeviceId { get; }

        public Guid DeviceTypeId { get; }

        public string ParamsJson { get; }
    }
}