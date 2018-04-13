using System;

namespace CardioMonitor.Devices.Data
{
    internal class DeviceConfigurationEntity
    {
        public Guid ConfigId { get; }

        public string ConfigName { get; }

        public Guid DeviceId { get; }

        public Guid DeviceTypeId { get; }

        public string ParamsJson { get; }

    }
}