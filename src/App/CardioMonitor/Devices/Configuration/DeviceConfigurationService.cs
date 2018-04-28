using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CardioMonitor.Devices.Data;
using JetBrains.Annotations;

namespace CardioMonitor.Devices.Configuration
{
    internal class DeviceConfigurationService : IDeviceConfigurationService
    {
        private readonly IDeviceConfigurationContextFactory _contextFactory;
        
        private readonly HashSet<Guid> _registeredDeviceIds;
        
        public DeviceConfigurationService(IDeviceConfigurationContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
            _registeredDeviceIds = new HashSet<Guid>();
        }
        
        public void RegisterDevice(Guid deviceId)
        {
            _registeredDeviceIds.Add(deviceId);
        }

        public void RegisterDevices(Guid[] deviceIds)
        {
            if (deviceIds == null) throw new ArgumentNullException(nameof(deviceIds));
            foreach (var deviceId in deviceIds)
            {
                _registeredDeviceIds.Add(deviceId);
            }
        }

        public Task<ICollection<DeviceConfiguration>> GetConfigurationsAsync()
        {
            return GetConfigurationAsync(x => _registeredDeviceIds.Contains(x.DeviceId));
        }

        private async Task<ICollection<DeviceConfiguration>> GetConfigurationAsync(
            [NotNull] Expression<Func<DeviceConfigurationEntity, bool>> expresion)
        {
            using (var context = _contextFactory.Create())
            {
                var result = await context.DeviceConfigurations.AsNoTracking()
                    .Where(expresion)
                    .ToListAsync()
                    .ConfigureAwait(false);

                return result
                    .Select(x => x.ToDomain())
                    .Where(x => x != null)
                    .ToList();
            }
        }

        public Task<ICollection<DeviceConfiguration>> GetConfigurationsAsync(Guid deviceTypeId)
        {
            return GetConfigurationAsync(x => _registeredDeviceIds.Contains(x.DeviceId)
                                              && x.DeviceTypeId == deviceTypeId);
        }
        
        public async Task<DeviceConfiguration> GetDeviceConfigurationAsync(Guid configId)
        {
            using (var context = _contextFactory.Create())
            {
                var result = await context.DeviceConfigurations.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ConfigId == configId
                                && _registeredDeviceIds.Contains(x.DeviceId))
                    .ConfigureAwait(false);

                return result?.ToDomain();
            }
        }

        public Task AddDeviceConfigurationAsync([NotNull] DeviceConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            using (var context = _contextFactory.Create())
            {
                var entity = config.ToEntity();

                //todo maybe set DeviceId expiclitly?

                context.DeviceConfigurations.Add(entity);

                return context.SaveChangesAsync();
            }
        }

        public async Task EditDeviceConfigurationAsync([NotNull] DeviceConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            using (var context = _contextFactory.Create())
            {
                var result = await context.DeviceConfigurations
                    .FirstOrDefaultAsync(x => x.ConfigId == config.ConfigId
                                              && _registeredDeviceIds.Contains(x.DeviceId))
                    .ConfigureAwait(false);

                if (result == null) throw new ArgumentException();
                
                result.DeviceId = config.DeviceId;
                result.ConfigId = config.ConfigId;
                result.DeviceTypeId = config.DeviceTypeId;
                result.ParamsJson = config.ParamsJson;

                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteDeviceConfigurationAsync(Guid confidId)
        { 
            using (var context = _contextFactory.Create())
            {
                var result = await context.DeviceConfigurations
                    .FirstOrDefaultAsync(x => x.ConfigId == confidId
                                              && _registeredDeviceIds.Contains(x.DeviceId))
                    .ConfigureAwait(false);

                if (result == null) throw new ArgumentException();

                context.DeviceConfigurations.Remove(result);

                await context.SaveChangesAsync();
            }
        }
    }
}