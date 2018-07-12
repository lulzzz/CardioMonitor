using System;
using JetBrains.Annotations;

namespace CardioMonitor.Devices.Data
{
    internal class DeviceConfigurationContextFactory : IDeviceConfigurationContextFactory
    {
        [NotNull]
        private readonly string _connectionString;

        public DeviceConfigurationContextFactory([NotNull] string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }


        public DeviceConfigurationContext Create()
        {
            return new DeviceConfigurationContext(_connectionString);
        }
    }
}