using System;
using System.Collections.Concurrent;
using System.Net;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Bed.UDP;
using CardioMonitor.Devices.Monitor;
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
            _controllers.TryGetValue(typeof(IBedController), out var controller);
            if (!(controller is IBedController badController))
            {
                badController = _CreateBedController();
                if (!_controllers.TryAdd(typeof(IBedController), badController))
                {
                    return null;
                }
            }

            return badController;
        }

        public IBedControllerInitParams CreateBedControllerInitParams(float maxAngleX, short cyclesCount, float movementFrequency)
        {
            return new BedUdpControllerInitParams(new IPEndPoint(new IPAddress(1),1 ),
                //todo fix this
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(2),
                maxAngleX,
                cyclesCount, 
                movementFrequency);
        }


        private IBedController _CreateBedController()
        {
            return new BedUDPController(_workerController);
        }
        
        public IMonitorController CreateMonitorController()
        {
            _controllers.TryGetValue(typeof(IMonitorController), out var controller);
            if (!(controller is IMonitorController badController))
            {
                badController = _CreateMonitorController();
                if (!_controllers.TryAdd(typeof(IMonitorController), badController))
                {
                    return null;
                }
            }

            return badController;
        }

        public IMonitorControllerInitParams CreateMonitorControllerInitParams()
        {
            return new MitarMonitorControlerInitParams(
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(2),
                20,
                20);
        }

        public TimeSpan? GetDeviceReconnectionTimeout()
        {
            throw new NotImplementedException();
        }

        private IMonitorController _CreateMonitorController()
        {
            //todo fix
            return null; //new OldMonitorController();
        }
    }
}