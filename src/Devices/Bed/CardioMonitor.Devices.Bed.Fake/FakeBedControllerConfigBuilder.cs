using System;
using CardioMonitor.Devices.Bed.Infrastructure;
using Newtonsoft.Json;

namespace CardioMonitor.Devices.Bed.Fake
{
    public class FakeBedControllerConfigBuilder : IBedControllerConfigBuilder
    {
        public IBedControllerConfig Build(
            string jsonConfig, 
            float maxAngleX = 0f,  
            short cyclesCount = 0, 
            float movementFrequency = 0f)
        {
            if (String.IsNullOrWhiteSpace(jsonConfig)) throw new ArgumentException(nameof(jsonConfig));

            var config = JsonConvert.DeserializeObject<JsonConfig>(jsonConfig);

            return new FakeBedControllerConfig(
                maxAngleX,
                cyclesCount,
                movementFrequency,
                TimeSpan.FromMilliseconds(config.UpdateDataPeriodMs),
                TimeSpan.FromMilliseconds(config.TimeoutMs),
                TimeSpan.FromMilliseconds(config.ConnectDelayMs),
                TimeSpan.FromMilliseconds(config.DisconnectDelayMs),
                TimeSpan.FromMilliseconds(config.DefaultDelayMs),
                TimeSpan.FromMilliseconds(config.CycleWithMaxAngleDurationMs),
                config.DeviceReconnectionTimeoutMs.HasValue
                    ? TimeSpan.FromMilliseconds(config.DeviceReconnectionTimeoutMs.Value)
                    : default(TimeSpan?));
        }

        public string Build(IBedControllerConfig config)
        {
            if (!(config is FakeBedControllerConfig fakeConfig)) throw new ArgumentException(nameof(config));

            var innerConfig = new JsonConfig
            {
                UpdateDataPeriodMs = fakeConfig.UpdateDataPeriod.TotalMilliseconds,
                ConnectDelayMs = fakeConfig.ConnectDelay.TotalMilliseconds,
                CycleWithMaxAngleDurationMs = fakeConfig.CycleWithMaxAngleDuration.TotalMilliseconds,
                DefaultDelayMs = fakeConfig.DefaultDelay.TotalMilliseconds,
                DeviceReconnectionTimeoutMs = fakeConfig.DeviceReconnectionTimeout?.TotalMilliseconds,
                DisconnectDelayMs = fakeConfig.DisconnectDelay.TotalMilliseconds,
                TimeoutMs = fakeConfig.Timeout.TotalMilliseconds
            };

            return JsonConvert.SerializeObject(innerConfig);
        }

        internal class JsonConfig
        {
            [JsonProperty("UpdateDataPeriodMs")]
            public double UpdateDataPeriodMs { get; set; }
            [JsonProperty("TimeoutMs")]
            public double TimeoutMs { get; set; }
            [JsonProperty("ConnectDelayMs")]
            public double ConnectDelayMs { get; set; }
            [JsonProperty("DisconnectDelayMs")]
            public double DisconnectDelayMs { get; set; }

            [JsonProperty("CycleWithMaxAngleDurationMs")]
            public double CycleWithMaxAngleDurationMs { get; set; }

            [JsonProperty("DefaultDelayMs")]
            public double DefaultDelayMs { get; set; }

            [JsonProperty("DeviceReconnectionTimeoutMs")]
            public double? DeviceReconnectionTimeoutMs { get; set; }
        }
    }
}