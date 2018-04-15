using System;
using CardioMonitor.Devices.Bed.Infrastructure;
using Newtonsoft.Json;

namespace CardioMonitor.Devices.Bed.Fake
{
    public class FakeBedControllerConfigBuilder : IBedControllerConfigBuilder
    {
        public IBedControllerConfig Build(float maxAngleX, short cyclesCount, float movementFrequency, string jsonConfig)
        {
            if (String.IsNullOrWhiteSpace(jsonConfig)) throw new ArgumentException(nameof(jsonConfig));

            var config = JsonConvert.DeserializeObject<FakeBedControllerConfig>(jsonConfig);

            config.MaxAngleX = maxAngleX;
            config.CyclesCount = cyclesCount;
            config.MovementFrequency = movementFrequency;

            return config;
        }
    }
}