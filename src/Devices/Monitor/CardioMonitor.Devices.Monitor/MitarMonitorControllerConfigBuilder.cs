using System;
using CardioMonitor.Devices.Monitor.Infrastructure;
using Newtonsoft.Json;

namespace CardioMonitor.Devices.Monitor
{
    public class MitarMonitorControllerConfigBuilder : IMonitorControllerConfigBuilder
    {
        public IMonitorControllerConfig Build(string jsonConfig)
        {
            if (String.IsNullOrWhiteSpace(jsonConfig)) throw new ArgumentException(nameof(jsonConfig));

            var innerConfig = JsonConvert.DeserializeObject<InternalMitarMonitorControlerConfig>(jsonConfig);

            return new MitarMonitorControlerConfig(
                TimeSpan.FromMilliseconds(innerConfig.UpdateDataPeriodMs),
                TimeSpan.FromMilliseconds(innerConfig.TimeoutMs),
                innerConfig.MonitorBroadcastUdpPort,
                innerConfig.MonitorTcpPort,
                innerConfig.DeviceReconectionsRetriesCount,
                innerConfig.DeviceReconnectionTimeoutMs.HasValue
                    ? TimeSpan.FromMilliseconds(innerConfig.DeviceReconnectionTimeoutMs.Value)
                    : default(TimeSpan?)
            );
        }

        public string Build(IMonitorControllerConfig config)
        {
            if (!(config is MitarMonitorControlerConfig typedConfig)) throw new ArgumentException(nameof(config));

            var innerConfig = new InternalMitarMonitorControlerConfig
            {
                UpdateDataPeriodMs = typedConfig.UpdateDataPeriod.TotalMilliseconds,
                DeviceReconnectionTimeoutMs = typedConfig.DeviceReconnectionTimeout?.TotalMilliseconds,
                DeviceReconectionsRetriesCount = typedConfig.DeviceReconectionsRetriesCount,
                TimeoutMs = typedConfig.Timeout.TotalMilliseconds,
                MonitorBroadcastUdpPort = typedConfig.MonitorBroadcastUdpPort,
                MonitorTcpPort = typedConfig.MonitorTcpPort
            };

            return JsonConvert.SerializeObject(innerConfig);
        }

        internal class InternalMitarMonitorControlerConfig
        {

            [JsonProperty("UpdateDataPeriod")]
            public double UpdateDataPeriodMs { get; set; }

            [JsonProperty("Timeout")]
            public double TimeoutMs { get; set; }

            [JsonProperty("DeviceReconnectionTimeout")]
            public double? DeviceReconnectionTimeoutMs { get; set; }
            
            [JsonProperty("DeviceReconectionsRetriesCount")]
            public int? DeviceReconectionsRetriesCount { get; set; }

            [JsonProperty("MonitorBroadcastUdpPort")]
            public int MonitorBroadcastUdpPort { get; set; }

            [JsonProperty("MonitorTcpPort")]
            public int MonitorTcpPort { get; set; }
            
            
        }
    }
}