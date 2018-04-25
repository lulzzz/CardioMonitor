﻿using System;
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
            TimeSpan pumpingDelay, 
            TimeSpan? deviceReconnectionTimeout = null)
        {
            UpdateDataPeriod = updateDataPeriod;
            Timeout = timeout;
            DefaultDelay = defaultDelay;
            PumpingDelay = pumpingDelay;
            DeviceReconnectionTimeout = deviceReconnectionTimeout;
        }

        [JsonProperty("UpdateDataPeriod")]
        public TimeSpan UpdateDataPeriod { get; }

        [JsonProperty("Timeout")]
        public TimeSpan Timeout { get; }

        /// <inheritdoc />
        [JsonProperty("DeviceReconnectionTimeout")]
        public TimeSpan? DeviceReconnectionTimeout { get; }

        [JsonProperty("DefaultDelay")]
        public TimeSpan DefaultDelay { get; }

        [JsonProperty("PumpingDelay")]
        public TimeSpan PumpingDelay { get;  }
    }
}