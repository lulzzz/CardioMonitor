using System;
using CardioMonitor.Devices.Bed.Infrastructure;
using Newtonsoft.Json;

namespace CardioMonitor.Devices.Bed.UDP
{
    public class BedUdpControllerConfigBuilder : IBedControllerConfigBuilder
    {
        public IBedControllerConfig Build(
            string jsonConfig, 
            float maxAngleX = 0f, 
            short cyclesCount = 0, 
            float movementFrequency = 0f)
        {
            if (String.IsNullOrWhiteSpace(jsonConfig)) throw new ArgumentException(nameof(jsonConfig));

            var config = JsonConvert.DeserializeObject<InternalBedUdpControllerConfig>(jsonConfig);

            return new BedUdpControllerConfig(config.BedIpEndpoint,
                TimeSpan.FromMilliseconds(config.UpdateDataPeriodMs),
                    TimeSpan.FromMilliseconds(config.TimeoutMs),
                maxAngleX, 
                cyclesCount, 
                movementFrequency,
                config.DeviceReconectionsRetriesCount,
                config.DeviceReconnectionTimeoutMs.HasValue
                ? TimeSpan.FromMilliseconds(config.DeviceReconnectionTimeoutMs.Value) 
                    :default(TimeSpan?));
        }

        public string Build(IBedControllerConfig config)
        {
            if (!(config is BedUdpControllerConfig typedConfig)) throw new ArgumentException(nameof(config));

            var innerConfig = new InternalBedUdpControllerConfig
            {
                BedIpEndpoint = typedConfig.BedIpEndpoint,
                UpdateDataPeriodMs = typedConfig.UpdateDataPeriod.TotalMilliseconds,
                TimeoutMs = typedConfig.Timeout.TotalMilliseconds,
                DeviceReconnectionTimeoutMs = typedConfig.DeviceReconnectionTimeout?.TotalMilliseconds,
                DeviceReconectionsRetriesCount = typedConfig.DeviceReconectionsRetriesCount
            };

            return JsonConvert.SerializeObject(innerConfig);
        }

        internal class InternalBedUdpControllerConfig
        {
            [JsonProperty("BedIpEndpoint")]
            public string BedIpEndpoint { get; set; }

            [JsonProperty("UpdateDataPeriod")]
            public double UpdateDataPeriodMs { get; set; }

            [JsonProperty("DeviceReconnectionTimeout")]
            public double? DeviceReconnectionTimeoutMs { get; set; }
            
            [JsonProperty("DeviceReconectionsRetriesCount")]
            public int? DeviceReconectionsRetriesCount { get; set; }

            [JsonProperty("Timeout")]
            public double TimeoutMs { get; set; }
        }
    }
}