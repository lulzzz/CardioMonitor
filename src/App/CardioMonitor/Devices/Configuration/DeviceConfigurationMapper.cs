using CardioMonitor.Devices.Data;

namespace CardioMonitor.Devices.Configuration
{
    internal static class DeviceConfigurationMapper
    {
        public static DeviceConfiguration ToDomain(this DeviceConfigurationEntity entity)
        {
            if (entity == null) return null;

            return new DeviceConfiguration
            {
                ConfigId = entity.ConfigId,
                ConfigName = entity.ConfigName,
                DeviceTypeId = entity.DeviceTypeId,
                DeviceId = entity.DeviceId,
                ParamsJson = entity.ParamsJson
            };
        }

        public static DeviceConfigurationEntity ToEntity(this DeviceConfiguration domain)
        {
            if (domain == null) return null;

            return new DeviceConfigurationEntity
            {
                ConfigId = domain.ConfigId,
                ConfigName = domain.ConfigName,
                DeviceTypeId = domain.DeviceTypeId,
                DeviceId = domain.DeviceId,
                ParamsJson = domain.ParamsJson
            };
        }
    }
}