using System;
using CardioMonitor.Devices.Bed.Infrastructure;
using Newtonsoft.Json;

namespace CardioMonitor.Devices.Bed.UDP
{
    public class BedUdpControllerConfigBuilder : IBedControllerConfigBuilder
    {
        public IBedControllerConfig Build(float maxAngleX, short cyclesCount, float movementFrequency, string jsonConfig)
        {
            if (String.IsNullOrWhiteSpace(jsonConfig)) throw new ArgumentException(nameof(jsonConfig));

            var config = JsonConvert.DeserializeObject<BedUdpControllerConfig>(jsonConfig);

            return new BedUdpControllerConfig(config.BedIpEndpoint,
                config.UpdateDataPeriod, 
                config.Timeout,
                maxAngleX, 
                cyclesCount, 
                movementFrequency,
                config.DeviceReconnectionTimeout);
        }
    }
}