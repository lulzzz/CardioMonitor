using System;
using System.Collections;
using System.Collections.Generic;
using CardioMonitor.Devices.Configuration;
using CardioMonitor.Devices.WpfModule;
using JetBrains.Annotations;
using SimpleInjector;

namespace CardioMonitor.Devices
{
    public interface IDeviceModulesController
    {
        void Register(DeviceTypeModule module);

        void RegisterDevice(WpfDeviceModule module);

        T GetViewModel<T>(Guid deviceId) where T: class, IDeviceControllerConfigViewModel;
        
        ICollection<DeviceTypeInfo> GetRegisteredDevicesTypes();

        ICollection<DeviceInfo> GetRegisteredDevices(Guid deviceTypeId);
    }

    public class DeviceModulesController : IDeviceModulesController
    {
        private readonly Container _container;
        private readonly IDeviceConfigurationService _deviceConfigurationService;
        private readonly Dictionary<Guid, WpfDeviceModule> _deviceModules;
        private readonly Dictionary<Guid, DeviceTypeModule> _deviceTypeModules;
        private readonly IDeviceControllerFactory _deviceControllerFactory;

        public DeviceModulesController(
            [NotNull] Container container,
            [NotNull] IDeviceConfigurationService deviceConfigurationService,
            [NotNull] IDeviceControllerFactory deviceControllerFactory)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _deviceConfigurationService = deviceConfigurationService ?? throw new ArgumentNullException(nameof(deviceConfigurationService));
            _deviceControllerFactory = deviceControllerFactory ?? throw new ArgumentNullException(nameof(deviceControllerFactory));
            _deviceModules = new Dictionary<Guid, WpfDeviceModule>();
            _deviceTypeModules = new Dictionary<Guid, DeviceTypeModule>();
        }


        public void Register([NotNull] DeviceTypeModule module)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));
            _deviceTypeModules[module.DeviceTypeId] = module;
        }

        public void RegisterDevice([NotNull] WpfDeviceModule module)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));
            if (!_deviceTypeModules.ContainsKey(module.DeviceTypeId)) throw new InvalidOperationException($"Device type with Id {module.DeviceTypeId} not registered");


            if (!((IList) module.DeviceControllerConfigViewModel.GetInterfaces()).Contains(typeof(IDeviceControllerConfigViewModel)))
                throw new InvalidOperationException($"type must implement {nameof(IDeviceControllerConfigViewModel)}");

            _deviceModules[module.DeviceId] = module;
            _deviceConfigurationService.RegisterDevice(module.DeviceId);
            _deviceControllerFactory.RegisterDevice(module.DeviceId, module.DeviceControllerType, module.DeviceControllerConfigBuilder);
        }

        public T GetViewModel<T>(Guid deviceId) where T: class, IDeviceControllerConfigViewModel
        {
            if (!_deviceModules.ContainsKey(deviceId)) throw new ArgumentException($"No device with id {deviceId}");

            var module = _deviceModules[deviceId];

            return _container.GetInstance(module.DeviceControllerConfigViewModel) as T;
        }


        public ICollection<DeviceTypeInfo> GetRegisteredDevicesTypes()
        {
            var result = new List<DeviceTypeInfo>(_deviceTypeModules.Count);
            foreach (var deviceType in _deviceTypeModules.Values)
            {
                result.Add(
                    new DeviceTypeInfo(deviceType.DeviceTypeName, deviceType.DeviceTypeId));

            }

            return result;
        }

        public ICollection<DeviceInfo> GetRegisteredDevices(Guid deviceTypeId)
        {
            var result = new List<DeviceInfo>(_deviceTypeModules.Count);
            foreach (var deviceType in _deviceModules.Values)
            {
                result.Add(
                    new DeviceInfo(deviceType.DeviceName, deviceType.DeviceId));
            }

            return result;
        }
    }
}