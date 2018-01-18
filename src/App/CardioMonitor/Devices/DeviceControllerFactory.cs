using System;
using System.Collections.Concurrent;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Bed.UDP;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure.Workers;
using CardioMonitor.Properties;

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
        [NotNull]
        private readonly ConcurrentDictionary<Type, IDeviceController> _controllers;
        
        [NotNull]
        private readonly IWorkerController _workerController;

        public DeviceControllerFactory([NotNull] IWorkerController workerController)
        {
            _workerController = workerController ?? throw new ArgumentNullException(nameof(workerController));
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
                badController = _CreateBedController();
                if (!_controllers.TryAdd(typeof(IBedController), badController))
                {
                    return null;
                }
            }

            return badController;
        }


        private IBedController _CreateBedController()
        {
            return new BedUDPController(_workerController);
        }
        
        public IMonitorController CreateMonitorController()
        {
            IDeviceController controller;
            _controllers.TryGetValue(typeof(IMonitorController), out controller);
            var badController = controller as IMonitorController;
            if (badController == null)
            {
                badController = _CreateMonitorController();
                if (!_controllers.TryAdd(typeof(IMonitorController), badController))
                {
                    return null;
                }
            }

            return badController;
        }
        private IMonitorController _CreateMonitorController()
        {
            //todo fix
            return null; //new OldMonitorController();
        }
    }
}