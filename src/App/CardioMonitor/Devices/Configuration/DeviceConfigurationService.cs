using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CardioMonitor.Devices.Bed.Fake;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Bed.UDP;
using CardioMonitor.Devices.Data;
using CardioMonitor.Devices.Monitor;
using CardioMonitor.Devices.Monitor.Fake;
using CardioMonitor.Devices.Monitor.Infrastructure;
using JetBrains.Annotations;

namespace CardioMonitor.Devices.Configuration
{
    internal class DeviceConfigurationService : IDeviceConfigurationService
    {
        private readonly IDeviceConfigurationContextFactory _contextFactory;

        private readonly ICollection<DeviceInfo> _supportedInversionTables;

        private readonly ICollection<DeviceInfo> _supportedCardioMonitors;


        private readonly HashSet<Guid> _registeredDeviceIds;//_supportedInversionTablesIds;

        private readonly HashSet<Guid> _registeredDeviceTypeIds;//_supportedCardioMonitorsIds;

        public DeviceConfigurationService(IDeviceConfigurationContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
            _registeredDeviceIds = new HashSet<Guid>();
            _registeredDeviceTypeIds = new HashSet<Guid>();
        }

        public void RegisterDevice(DeviceRegistrationInfo info)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<DeviceConfiguration>> GetConfigurationsAsync()
        {
            using (var context = _contextFactory.Create())
            {
                var result = await context.DeviceConfigurations.AsNoTracking()
                    .Where(x => _registeredDeviceIds.Contains(x.DeviceId))
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
            using (var context = _contextFactory.Create())
            {
                var result = await context.DeviceConfigurations.AsNoTracking()
                    .Where(x => _registeredDeviceIds.Contains(x.DeviceId)
                            && x.DeviceTypeId == deviceTypeId)
                    .ToListAsync()
                    .ConfigureAwait(false);

                return result
                    .Select(x => x.ToDomain())
                    .Where(x => x != null)
                    .ToList();
            }
        }

        
        public Task<ICollection<DeviceInfo>> GetRegisteredDevicesAsync()
        {
            return Task.FromResult(_supportedInversionTables);
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
                var result = await context.DeviceConfigurations.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ConfigId == config.ConfigId
                                              && _supportedInversionTablesIds.Contains(x.DeviceId))
                    .ConfigureAwait(false);

                if (result == null) throw new ArgumentException();

                var entity = config.ToEntity();
                context.DeviceConfigurations.Attach(entity);
                context.Entry(entity).State = EntityState.Modified;

                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteDeviceConfigurationAsync(Guid confidId)
        { 

            using (var context = _contextFactory.Create())
            {
                var result = await context.DeviceConfigurations.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ConfigId == confidId
                                              && _supportedInversionTablesIds.Contains(x.DeviceId))
                    .ConfigureAwait(false);

                if (result == null) throw new ArgumentException();

                context.DeviceConfigurations.Remove(result);

                await context.SaveChangesAsync();
            }
        }
    }
}