using System;
using CardioMonitor.Devices.Monitor.Infrastructure;

namespace CardioMonitor.Devices.Monitor
{
    /// <summary>
    /// Параметры подкючения к монитору МИТАР
    /// </summary>
    public class MitarMonitorControlerInitParams : IMonitorControllerInitParams
    {

        public MitarMonitorControlerInitParams(
            TimeSpan updateDataPeriod,
            TimeSpan timeout,
            int monitorBroadcastUdpPort,
            int monitorTcpPort)
        {
            UpdateDataPeriod = updateDataPeriod;
            Timeout = timeout;
            MonitorBroadcastUdpPort = monitorBroadcastUdpPort;
            MonitorTcpPort = monitorTcpPort;
            //todo нужно еще задавать IP
        }

        /// <summary>
        /// Период обмена сообщениями с устройством
        /// </summary>
        public TimeSpan UpdateDataPeriod { get; }

        /// <summary>
        /// Таймаут операций
        /// </summary>
        public TimeSpan Timeout { get; }

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
    }
}