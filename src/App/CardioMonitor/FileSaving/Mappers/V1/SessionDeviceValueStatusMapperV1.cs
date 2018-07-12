using System;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.FileSaving.Containers.V1;

namespace CardioMonitor.FileSaving.Mappers.V1
{
    internal static class SessionDeviceValueStatusMapperV1
    {
        public static DeviceValueStatus ToDomain(this StoredDeviceValueStatusV1 status)
        {
            switch (status)
            {
                case StoredDeviceValueStatusV1.Unknown:
                    return DeviceValueStatus.Unknown;
                case StoredDeviceValueStatusV1.NotObtained:
                    return DeviceValueStatus.NotObtained;
                case StoredDeviceValueStatusV1.Obtained:
                    return DeviceValueStatus.Obtained;
                case StoredDeviceValueStatusV1.ErrorOccured:
                    return DeviceValueStatus.ErrorOccured;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
        
        public static StoredDeviceValueStatusV1 ToStored(this DeviceValueStatus  status)
        {
            switch (status)
            {
                case DeviceValueStatus.Unknown:
                    return StoredDeviceValueStatusV1.Unknown;
                case DeviceValueStatus.NotObtained:
                    return StoredDeviceValueStatusV1.NotObtained;
                case DeviceValueStatus.Obtained:
                    return StoredDeviceValueStatusV1.Obtained;
                case DeviceValueStatus.ErrorOccured:
                    return StoredDeviceValueStatusV1.ErrorOccured;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }
}