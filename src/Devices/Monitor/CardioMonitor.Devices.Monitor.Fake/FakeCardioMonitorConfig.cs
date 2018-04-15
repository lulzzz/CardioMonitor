using System;
using CardioMonitor.Devices.Monitor.Infrastructure;
using Newtonsoft.Json;

namespace CardioMonitor.Devices.Monitor.Fake
{
    public class FakeCardioMonitorConfig : IMonitorControllerConfig
    {
        public FakeCardioMonitorConfig(
            TimeSpan updateDataPeriod, 
            TimeSpan timeout, 
            TimeSpan defaultDelay, 
            TimeSpan pumpingDelay)
        {
            UpdateDataPeriod = updateDataPeriod;
            Timeout = timeout;
            DefaultDelay = defaultDelay;
            PumpingDelay = pumpingDelay;
        }

        [JsonProperty("UpdateDataPeriod")]
        public TimeSpan UpdateDataPeriod { get; }

        [JsonProperty("Timeout")]
        public TimeSpan Timeout { get; }

        [JsonProperty("DefaultDelay")]
        public TimeSpan DefaultDelay { get; }

        [JsonProperty("PumpingDelay")]
        public TimeSpan PumpingDelay { get;  }
    }
}