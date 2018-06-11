using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CardioMonitor.Devices.Data;
using CardioMonitor.Events.Devices;
using JetBrains.Annotations;
using Markeli.Utils.EventBus.Contracts;

namespace CardioMonitor.Devices.Configuration
{
    internal class DeviceConfigurationService : IDeviceConfigurationService
    {
        [NotNull]
        private readonly IDeviceConfigurationContextFactory _contextFactory;
        
        private readonly HashSet<Guid> _registeredDeviceIds;

        [NotNull]
        private readonly IEventBus _eventBus;

        public DeviceConfigurationService(
            [NotNull] IDeviceConfigurationContextFactory contextFactory, 
            [NotNull] IEventBus eventBus)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
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

        public async Task<Guid> AddDeviceConfigurationAsync([NotNull] DeviceConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            using (var context = _contextFactory.Create())
            {
                var entity = config.ToEntity();

                //todo maybe set DeviceId expiclitly?

                context.DeviceConfigurations.Add(entity);

                await context
                    .SaveChangesAsync()
                    .ConfigureAwait(false);

                await _eventBus
                    .PublishAsync(new DeviceConfigAddedEvent(entity.ConfigId))
                    .ConfigureAwait(false);

                return entity.ConfigId;
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
                context.DeviceConfigurations.Attach(result);
                
                await context.SaveChangesAsync();
                
                await _eventBus
                    .PublishAsync(new DeviceConfigChangedEvent(config.ConfigId))
                    .ConfigureAwait(false);
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
                
                await _eventBus
                    .PublishAsync(new DeviceConfigDeletedEvent(confidId))
                    .ConfigureAwait(false);
            }
        }
    }
}