using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CardioMonitor.Devices.Configuration
{
    public interface IDeviceConfigurationService
    {
        Task<ICollection<DeviceConfiguration>> GetAllDeviceConfigurationsAsync();
        
        Task<ICollection<DeviceInfo>> GetRegisterdMonitorsAsync();

        Task<ICollection<DeviceInfo>> GetRegisteredInversionTablesAsync();

        Task<DeviceConfiguration> GetDeviceConfigurationAsync(Guid configId);

        Task AddDeviceConfigurationAsync(DeviceConfiguration config);

        Task EditDeviceConfigurationAsync(DeviceConfiguration config);

        Task DeleteDeviceConfigurationAsync(Guid confidId);
    }
}