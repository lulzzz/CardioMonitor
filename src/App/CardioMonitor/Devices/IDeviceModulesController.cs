using System;
using CardioMonitor.Devices.Configuration;
using CardioMonitor.Devices.WpfModule;
using JetBrains.Annotations;
using SimpleInjector;

namespace CardioMonitor.Devices
{
    public interface IDeviceModulesController
    {
        void Register(IWpfDeviceModule module);

        IDeviceControllerConfigViewModel GetViewModel(Guid deviceId);

        IDeviceControllerConfigBuilder GetConfigBuilder(Guid deviceId);
    }

    public class DeviceModulesController : IDeviceModulesController
    {
        private readonly Container _container;
        private readonly IDeviceConfigurationService _deviceConfigurationService;

        public DeviceModulesController([NotNull] Container container,
            [NotNull] IDeviceConfigurationService deviceConfigurationService)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _deviceConfigurationService = deviceConfigurationService ?? throw new ArgumentNullException(nameof(deviceConfigurationService));
        }


        public void Register(IWpfDeviceModule module)
        {
            throw new NotImplementedException();
        }

        public IDeviceControllerConfigViewModel GetViewModel(Guid deviceId)
        {
            throw new NotImplementedException();
        }

        public IDeviceControllerConfigBuilder GetConfigBuilder(Guid deviceId)
        {
            throw new NotImplementedException();
        }
    }
}