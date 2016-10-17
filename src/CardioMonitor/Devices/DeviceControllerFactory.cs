using System;
using System.Collections.Concurrent;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Bed.Usb;

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
        private readonly ConcurrentDictionary<Type, IDeviceController> _controllers;

        public DeviceControllerFactory()
        {
            _controllers = new ConcurrentDictionary<Type, IDeviceController>();
        }

        /// <summary>
        /// Возвращает контроллер для взаимодействия с кроватью
        /// </summary>
        /// <returns></returns>
        public IBedController CreateBedController()
        {
            IDeviceController controller;
            _controllers.TryGetValue(typeof(IBedController), out controller);
            var badController = controller as IBedController;
            if (badController == null)
            {
                badController = CreaBedController();
                if (!_controllers.TryAdd(typeof(IBedController), badController))
                {
                    return null;
                }
            }

            return badController;
        }

        private IBedController CreaBedController()
        {
            return new BedUsbController();
        }
    }
}