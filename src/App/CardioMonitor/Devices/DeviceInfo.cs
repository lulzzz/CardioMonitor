using System;

namespace CardioMonitor.Devices
{
    public class DeviceInfo
    {
        public DeviceInfo(string name, Guid deviceId)
        {
            Name = name;
            DeviceId = deviceId;
        }

        public string Name { get; }

        public Guid DeviceId { get;  }
    }
}