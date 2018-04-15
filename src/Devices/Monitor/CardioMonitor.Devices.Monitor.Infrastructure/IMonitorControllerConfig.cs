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
    }
}