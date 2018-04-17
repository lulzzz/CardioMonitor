using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CardioMonitor.Devices
{
    /// <summary>
    /// Фабрика по созданию контроллеров для работы с устройствами. 
    /// </summary>
    public interface IDeviceControllerFactory
    {
        void RegisterDevice(Guid deviceId, [NotNull] Type controllerType, [NotNull] Type configBuilderType);

        /// <summary>
        /// Создает контроллер взаимодействия с устройством на основе конфига
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        /// <remarks>
        /// Конфиг получается из <see cref="Configuration.IDeviceConfigurationService"/>
        /// </remarks>
        [NotNull]
        T CreateDeviceController<T>(Guid deviceId)
            where T : class, IDeviceController;

        T CreateDeviceControllerConfigBuilder<T>(Guid deviceId)
            where T : class, IDeviceControllerConfigBuilder;
    }
}