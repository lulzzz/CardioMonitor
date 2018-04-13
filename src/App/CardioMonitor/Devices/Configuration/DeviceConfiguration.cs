using System;

namespace CardioMonitor.Devices.Configuration
{
    public class DeviceConfiguration
    {
        public Guid ConfigId { get; set; }

        public string ConfigName { get; set; }

        public Guid DeviceId { get; set; }

        public Guid DeviceTypeId { get; set; }

        public string ParamsJson { get; set; }
    }
}