using System;
using CardioMonitor.Devices.Monitor.Infrastructure;

namespace CardioMonitor.Devices.Monitor
{
    /// <summary>
    /// Параметры подкючения к монитору МИТАР
    /// </summary>
    public class MitarMonitorControlerConfig : IMonitorControllerConfig
    {

        public MitarMonitorControlerConfig(
            TimeSpan updateDataPeriod,
            TimeSpan timeout,
            int monitorBroadcastUdpPort,
            int monitorTcpPort, int? deviceReconectionsRetriesCount, TimeSpan? deviceReconnectionTimeout = null)
        {
            UpdateDataPeriod = updateDataPeriod;
            Timeout = timeout;
            MonitorBroadcastUdpPort = monitorBroadcastUdpPort;
            MonitorTcpPort = monitorTcpPort;
            DeviceReconectionsRetriesCount = deviceReconectionsRetriesCount;
            DeviceReconnectionTimeout = deviceReconnectionTimeout;
            //todo нужно еще задавать IP //todo is it true?
        }

        /// <summary>
        /// Период обмена сообщениями с устройством
        /// </summary>
        public TimeSpan UpdateDataPeriod { get; }

        /// <summary>
        /// Таймаут операций
        /// </summary>
        public TimeSpan Timeout { get; }

        /// <inheritdoc />
        public TimeSpan? DeviceReconnectionTimeout { get; }

        /// <summary>
        /// Порт, по которому будут приходить broadcast сообщения от монитора
        /// </summary>
        public int MonitorBroadcastUdpPort { get; }

        /// <summary>
        /// Порт для TCP подключения к монитору
        /// </summary>
        /// <remarks>
        /// IP можно узнать в результе broadcast'a от устройства, который надо слушать по порту <see cref="MonitorBroadcastUdpPort"/>
        /// </remarks>
        public int MonitorTcpPort { get; }
        
        // <inheritdoc />
        public int? DeviceReconectionsRetriesCount { get; }
    }
}