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

            return JsonConvert.DeserializeObject<MitarMonitorControlerConfig>(jsonConfig);
        }
    }
}