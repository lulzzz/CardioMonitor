using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardioMonitor.Devices.Configuration;
using CardioMonitor.Properties;
using SimpleInjector;

namespace CardioMonitor.Devices
{
    /// <summary>
    /// Фабрика по созданию контроллеров для работы с устройствами. 
    /// Возвращает кэшированный контроллер, если его нет в кэше - будет создан новый и помещен в кэш
    /// </summary>
    /// <remarks>
    /// Создана для того, чтобы иметь единую точку доступа ко всем устройствам, с которым придется работать. 
    /// </remarks>
    public class DeviceControllerFactory : IDeviceControllerFactory
    {
        private readonly Dictionary<Guid, Tuple<Type, Type>> _deviceControllers;

        [NotNull]
        private readonly IDeviceConfigurationService _configurationService;

        [NotNull]
        private readonly Container _container;


        public DeviceControllerFactory(
            [NotNull] IDeviceConfigurationService configurationService,
            [NotNull] Container container)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _deviceControllers = new Dictionary<Guid, Tuple<Type, Type>>();
        }

        public void RegisterDevice(Guid deviceId, Type controllerType, Type configBuilderType)
        {
            if (controllerType == null) throw new ArgumentNullException(nameof(controllerType));
            if (configBuilderType == null) throw new ArgumentNullException(nameof(configBuilderType));

            if (!controllerType.GetInterfaces().Contains(typeof(IDeviceController)))
                throw new InvalidOperationException($"type must implement {nameof(IDeviceController)}");

            if (!configBuilderType.GetInterfaces().Contains(typeof(IDeviceControllerConfigBuilder)))
                throw new InvalidOperationException($"type must implement {nameof(IDeviceControllerConfigBuilder)}");

            _deviceControllers[deviceId] = new Tuple<Type, Type>(controllerType, configBuilderType);
        }

        public async Task<T> CreateDeviceControllerAsync<T>(Guid configId) where T: class, IDeviceController
        {
            var config = await _configurationService
                .GetDeviceConfigurationAsync(configId)
                .ConfigureAwait(false);
            if (config == null) throw new ArgumentException($"No config with Id {configId}");

            if (!_deviceControllers.ContainsKey(config.DeviceId)) throw new InvalidOperationException($"Device with Id {config.DeviceId} not registered");

            var deviceInfo = _deviceControllers[config.DeviceId];
            return _container.GetInstance(deviceInfo.Item1) as T;
        }

        public async Task<T> CreateDeviceControllerConfigBuilderAsync<T>(Guid configId) where T : class, IDeviceControllerConfigBuilder
        {
            var config = await _configurationService
                .GetDeviceConfigurationAsync(configId)
                .ConfigureAwait(false);
            if (config == null) throw new ArgumentException($"No config with Id {configId}");

            if (!_deviceControllers.ContainsKey(config.DeviceId)) throw new InvalidOperationException($"Device with Id {config.DeviceId} not registered");

            var deviceInfo = _deviceControllers[config.DeviceId];
            return _container.GetInstance(deviceInfo.Item1) as T;
        }
    }
}