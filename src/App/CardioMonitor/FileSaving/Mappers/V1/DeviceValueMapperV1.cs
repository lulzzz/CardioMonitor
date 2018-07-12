using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.FileSaving.Containers.V1;

namespace CardioMonitor.FileSaving.Mappers.V1
{
    internal static class DeviceValueMapperV1
    {
        public static DeviceValue<T> ToDomain<T>(this StoredDeviceValueV1<T> deviceValue)
        {
            return new DeviceValue<T>(deviceValue.Value, deviceValue.Status.ToDomain());
        }
        
        public static StoredDeviceValueV1<T> ToStored<T>(this DeviceValue<T> deviceValue)
        {
            return new StoredDeviceValueV1<T>
            {
                Value = deviceValue.Value,
                Status = deviceValue.Status.ToStored()
            };
        }
    }
}