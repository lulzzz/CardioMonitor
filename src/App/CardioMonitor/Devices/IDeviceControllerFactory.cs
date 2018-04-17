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
        void RegisterDevice(Guid deviceId, Type controllerType, Type configBuilderType);

        /// <summary>
        /// Создает контроллер взаимодействия с устройством на основе конфига
        /// </summary>
        /// <param name="configId"></param>
        /// <returns></returns>
        /// <remarks>
        /// Конфиг получается из <see cref="Configuration.IDeviceConfigurationService"/>
        /// </remarks>
        IDeviceController CreateDeviceController(Guid configId);
    }
}