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
        /// <param name="configId"></param>
        /// <returns></returns>
        /// <remarks>
        /// Конфиг получается из <see cref="Configuration.IDeviceConfigurationService"/>
        /// </remarks>
        [NotNull]
        Task<T> CreateDeviceControllerAsync<T>(Guid configId)
            where T : class, IDeviceController;

        Task<T> CreateDeviceControllerConfigBuilderAsync<T>(Guid configId)
            where T : class, IDeviceControllerConfigBuilder;
    }
}