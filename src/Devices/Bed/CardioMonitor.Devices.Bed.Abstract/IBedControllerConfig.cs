using System;

namespace CardioMonitor.Devices.Bed.Infrastructure
{
    /// <summary>
    /// Параметры инициализации контроллера взаимодействия с инверсионным столом
    /// </summary>
    /// <remarks>
    /// Параметры могут отличаться в зависимости от способ подключения к устройству
    /// </remarks>
    public interface IBedControllerConfig : IDeviceControllerConfig
    {
        /// <summary>
        /// Таймаут операций
        /// </summary>
        TimeSpan Timeout { get; }
        
        /// <summary>
        /// Время, через которое будет осуществляться попытка переподключения к устройству
        /// </summary>
        /// <remarks>
        /// Если null, то переподключения не будет
        /// </remarks>
        TimeSpan? DeviceReconnectionTimeout { get; }
        
        /// <summary>
        /// Количество попыток переподключения
        /// </summary>
        /// <remarks>
        /// Если null, то переподключения не будет
        /// </remarks>
        int? DeviceReconectionsRetriesCount { get; }
    }
}