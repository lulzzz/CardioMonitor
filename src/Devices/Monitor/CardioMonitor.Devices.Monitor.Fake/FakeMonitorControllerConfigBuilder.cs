using System;
using CardioMonitor.Devices.Monitor.Infrastructure;
using Newtonsoft.Json;

namespace CardioMonitor.Devices.Monitor.Fake
{
    public class FakeMonitorControllerConfigBuilder : IMonitorControllerConfigBuilder
    {
        public IMonitorControllerConfig Build(string jsonConfig)
        {
            if (String.IsNullOrWhiteSpace(jsonConfig)) throw new ArgumentException(nameof(jsonConfig));

            var innerConfig = JsonConvert.DeserializeObject<InternalFakeCardioMinotrConfig>(jsonConfig);

            return new FakeCardioMonitorConfig(
                TimeSpan.FromMilliseconds(innerConfig.UpdateDataPeriodMs),
                TimeSpan.FromMilliseconds(innerConfig.TimeoutMs),
                TimeSpan.FromMilliseconds(innerConfig.DefaultDelayMs),
                TimeSpan.FromMilliseconds(innerConfig.PumpingDelayMs),
                innerConfig.DeviceReconectionsRetriesCount,
                innerConfig.DeviceReconnectionTimeoutMs.HasValue
                    ? TimeSpan.FromMilliseconds(innerConfig.DeviceReconnectionTimeoutMs.Value)
                    : default(TimeSpan?));
        }

        public string Build(IMonitorControllerConfig config)
        {
            if (!(config is FakeCardioMonitorConfig typedConfig)) throw new ArgumentException(nameof(config));

            var innerConfig = new InternalFakeCardioMinotrConfig
            {
                UpdateDataPeriodMs = typedConfig.UpdateDataPeriod.TotalMilliseconds,
                TimeoutMs = typedConfig.Timeout.TotalMilliseconds,
                DefaultDelayMs = typedConfig.DefaultDelay.TotalMilliseconds,
                PumpingDelayMs = typedConfig.PumpingDelay.TotalMilliseconds,
                DeviceReconnectionTimeoutMs = typedConfig.DeviceReconnectionTimeout?.TotalMilliseconds,
                DeviceReconectionsRetriesCount = typedConfig.DeviceReconectionsRetriesCount
            };

            return JsonConvert.SerializeObject(innerConfig);
        }

        internal class InternalFakeCardioMinotrConfig
        {
            [JsonProperty("UpdateDataPeriod")]
            public double UpdateDataPeriodMs { get; set; }

            [JsonProperty("Timeout")]
            public double TimeoutMs { get; set; }
            
            [JsonProperty("DeviceReconnectionTimeout")]
            public double? DeviceReconnectionTimeoutMs { get; set; }
            
            // <inheritdoc />
            [JsonProperty("DeviceReconectionsRetriesCount")]
            public int? DeviceReconectionsRetriesCount { get; set; }

            [JsonProperty("DefaultDelay")]
            public double DefaultDelayMs { get; set; }

            [JsonProperty("PumpingDelay")]
            public double PumpingDelayMs { get; set; }
        }
    }
}