using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CardioMonitor.Devices.Configuration
{
    public interface IDeviceConfigurationService
    {
        void RegisterDevice(Guid deviceId);
        void RegisterDevices([NotNull] Guid[] deviceIds);

        Task<ICollection<DeviceConfiguration>> GetConfigurationsAsync();

        Task<ICollection<DeviceConfiguration>> GetConfigurationsAsync(Guid deviceTypeId);


        Task<DeviceConfiguration> GetDeviceConfigurationAsync(Guid configId);

        Task<Guid> AddDeviceConfigurationAsync(DeviceConfiguration config);

        Task EditDeviceConfigurationAsync(DeviceConfiguration config);

        Task DeleteDeviceConfigurationAsync(Guid confidId);
    }

    public class DeviceRegistrationInfo
    {
        public DeviceRegistrationInfo(
            string name, 
            Guid deviceId, 
            Guid deviceTypeId)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            Name = name;
            DeviceId = deviceId;
            DeviceTypeId = deviceTypeId;
        }

        public string Name { get; }

        public Guid DeviceId { get; }

        public Guid DeviceTypeId { get; }
    }
}