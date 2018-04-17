using System;

namespace CardioMonitor.Devices
{
    public class DeviceTypeInfo
    {
        public DeviceTypeInfo(string name, Guid deviceTypeId)
        {
            Name = name;
            DeviceTypeId = deviceTypeId;
        }

        public string Name { get; }

        public Guid DeviceTypeId { get; }
    }
}