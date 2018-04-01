using System;
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
        private readonly IWorkerController _workerController;

        public DeviceControllerFactory([NotNull] IWorkerController workerController)
        {
            _workerController = workerController ?? throw new ArgumentNullException(nameof(workerController));
        }

        /// <summary>
        /// Возвращает контроллер для взаимодействия с кроватью
        /// </summary>
        /// <returns></returns>
        public IBedController CreateBedController()
        {

            return new BedUDPController(_workerController);
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

        
        
        public IMonitorController CreateMonitorController()
        {
            //todo fix
            return null; //new OldMonitorController();
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
            //todo get value from settings
            return null;
        }        
    }
}