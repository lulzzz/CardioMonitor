using System;
using CardioMonitor.Devices.Monitor.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
            int monitorTcpPort, 
            TimeSpan? deviceReconnectionTimeout = null)
        {
            UpdateDataPeriod = updateDataPeriod;
            Timeout = timeout;
            MonitorBroadcastUdpPort = monitorBroadcastUdpPort;
            MonitorTcpPort = monitorTcpPort;
            DeviceReconnectionTimeout = deviceReconnectionTimeout;
            //todo нужно еще задавать IP
        }

        /// <summary>
        /// Период обмена сообщениями с устройством
        /// </summary>
        [JsonProperty("UpdateDataPeriod")]
        public TimeSpan UpdateDataPeriod { get; }

        /// <summary>
        /// Таймаут операций
        /// </summary>
        [JsonProperty("Timeout")]
        public TimeSpan Timeout { get; }

        /// <inheritdoc />
        [JsonProperty("DeviceReconnectionTimeout")]
        public TimeSpan? DeviceReconnectionTimeout { get; }

        /// <summary>
        /// Порт, по которому будут приходить broadcast сообщения от монитора
        /// </summary>
        [JsonProperty("MonitorBroadcastUdpPort")]
        public int MonitorBroadcastUdpPort { get; }

        /// <summary>
        /// Порт для TCP подключения к монитору
        /// </summary>
        /// <remarks>
        /// IP можно узнать в результе broadcast'a от устройства, который надо слушать по порту <see cref="MonitorBroadcastUdpPort"/>
        /// </remarks>
        [JsonProperty("MonitorTcpPort")]
        public int MonitorTcpPort { get; }
    }
}