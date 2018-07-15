using System;

namespace CardioMonitor.Devices.Monitor.Infrastructure
{
    /// <summary>
    /// Параметры инициализации контроллера взаимодействия с монитором
    /// </summary>
    public interface IMonitorControllerConfig : IDeviceControllerConfig
    {
        /// <summary>
        /// Период обмена сообщениями с устройством
        /// </summary>
        TimeSpan UpdateDataPeriod { get; }

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