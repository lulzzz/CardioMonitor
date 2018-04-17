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
        private readonly Container _container;


        public DeviceControllerFactory(
            [NotNull] Container container)
        {
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

        public T CreateDeviceController<T>(Guid deviceId) where T: class, IDeviceController
        {
            if (!_deviceControllers.ContainsKey(deviceId)) throw new InvalidOperationException($"Device with Id {deviceId} not registered");

            var deviceInfo = _deviceControllers[deviceId];
            return _container.GetInstance(deviceInfo.Item1) as T ?? throw new InvalidOperationException();
        }

        public T CreateDeviceControllerConfigBuilder<T>(Guid deviceId) where T : class, IDeviceControllerConfigBuilder
        {
            if (!_deviceControllers.ContainsKey(deviceId)) throw new InvalidOperationException($"Device with Id {deviceId} not registered");

            var deviceInfo = _deviceControllers[deviceId];
            return _container.GetInstance(deviceInfo.Item1) as T;
        }
    }
}