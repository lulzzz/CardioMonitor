using System;
using CardioMonitor.Devices.Bed.Infrastructure;
using Newtonsoft.Json;

namespace CardioMonitor.Devices.Bed.Fake
{
    public class FakeBedControllerConfig : IBedControllerConfig
    {
        public float MaxAngleX { get; set; }
        public short CyclesCount { get; set; }
        public float MovementFrequency { get; set; }

        [JsonProperty("UpdateDataPeriod")]
        public TimeSpan UpdateDataPeriod { get; set; }

        [JsonProperty("Timeout")]
        public TimeSpan Timeout { get; set; }
    }
}