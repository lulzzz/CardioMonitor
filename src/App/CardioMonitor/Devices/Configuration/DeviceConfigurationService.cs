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


        private readonly HashSet<Guid> _supportedInversionTablesIds;

        private readonly HashSet<Guid> _supportedCardioMonitorsIds;

        public DeviceConfigurationService(IDeviceConfigurationContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
            _supportedInversionTables = new List<DeviceInfo>
            {
                new DeviceInfo
                {
                    Name = "Эмулятор инверсионного стола",
                    DeviceId = FakeInversionTableDeviceId.DeviceId
                },
                new DeviceInfo
                {
                    Name = "Инверсионный стол v2",
                    DeviceId = InversionTableV2UdpDeviceId.DeviceId
                },
            };
            _supportedInversionTablesIds = new HashSet<Guid>(_supportedInversionTables.Select(x => x.DeviceId));
            _supportedCardioMonitors = new List<DeviceInfo>
            {
                new DeviceInfo
                {
                    Name = "Эмулятор кардио монитора",
                    DeviceId = FakeMonitorDeviceId.DeviceId
                },
                new DeviceInfo
                {
                    Name = "Митар 01 «Р-Д»",
                    DeviceId = MitarMonitorDeviceId.DeviceId
                },
            };
            _supportedCardioMonitorsIds = new HashSet<Guid>(_supportedInversionTables.Select(x => x.DeviceId));
        }

        public async Task<ICollection<DeviceConfiguration>> GetMonitorsConfigurationsAsync()
        {
            using (var context = _contextFactory.Create())
            {
                var result = await context.DeviceConfigurations.AsNoTracking()
                    .Where(x => x.DeviceTypeId == MonitorDeviceTypeId.DeviceTypeId
                                && _supportedCardioMonitorsIds.Contains(x.DeviceId))
                    .ToListAsync()
                    .ConfigureAwait(false);

                return result
                    .Select(x => x.ToDomain())
                    .Where(x => x != null)
                    .ToList();
            }
        }

      
        public async Task<ICollection<DeviceConfiguration>> GetInversionTablesConfigurationsAsync()
        {
            using (var context = _contextFactory.Create())
            {
                var result = await context.DeviceConfigurations.AsNoTracking()
                    .Where(x => x.DeviceTypeId == InversionTableDeviceTypeId.DeviceTypeId
                                && _supportedInversionTablesIds.Contains(x.DeviceId))
                    .ToListAsync()
                    .ConfigureAwait(false);

                return result
                    .Select(x => x.ToDomain())
                    .Where(x => x != null)
                    .ToList();
            }
        }

        public Task<ICollection<DeviceInfo>> GetRegisterdMonitorsAsync()
        {
            return Task.FromResult(_supportedCardioMonitors);
        }

        public Task<ICollection<DeviceInfo>> GetRegisteredInversionTablesAsync()
        {
            return Task.FromResult(_supportedInversionTables);
        }

        public async Task<DeviceConfiguration> GetDeviceConfigurationAsync(Guid configId)
        {
            using (var context = _contextFactory.Create())
            {
                var result = await context.DeviceConfigurations.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ConfigId == configId
                                && _supportedInversionTablesIds.Contains(x.DeviceId))
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