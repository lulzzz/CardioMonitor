using System;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Monitor.Infrastructure;

namespace CardioMonitor.Devices
{
    /// <summary>
    /// Фабрика по созданию контроллеров для работы с устройствами. 
    /// Возвращает кэшированный контроллер, если его нет в кэше - будет создан новый и помещен в кэш
    /// </summary>
    /// <remarks>
    /// Создана для того, чтобы иметь единую точку доступа ко всем устройствам, с которым придется работать. 
    /// </remarks>
    public interface IDeviceControllerFactory
    {
        /// <summary>
        /// Возвращает контроллер для взаимодействия с кроватью
        /// </summary>
        /// <returns></returns>
        IBedController CreateBedController();

        IBedControllerConfig CreateBedControllerInitParams(float maxAngleX, short cyclesCount, float movementFrequency);

        IMonitorController CreateMonitorController();
        
        IMonitorControllerConfig CreateMonitorControllerInitParams();

        /// <summary>
        /// Возвращает время, через которое будет осуществляться попытка переподключения к устройству
        /// </summary>
        /// <remarks>
        /// Если null, то переподключения не будет
        /// </remarks>
        TimeSpan? GetDeviceReconnectionTimeout();
    }
}