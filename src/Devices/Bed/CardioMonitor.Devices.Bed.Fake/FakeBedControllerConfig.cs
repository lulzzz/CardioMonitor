﻿using System;
using CardioMonitor.Devices.Bed.Infrastructure;
using Newtonsoft.Json;

namespace CardioMonitor.Devices.Bed.Fake
{
    public class FakeBedControllerConfig : IBedControllerConfig
    {
        public FakeBedControllerConfig(
            float maxAngleX, 
            short cyclesCount, 
            float movementFrequency, 
            TimeSpan updateDataPeriod, 
            TimeSpan timeout,
            TimeSpan? deviceReconnectionTimeout = null)
        {
            MaxAngleX = maxAngleX;
            CyclesCount = cyclesCount;
            MovementFrequency = movementFrequency;
            UpdateDataPeriod = updateDataPeriod;
            Timeout = timeout;
            DeviceReconnectionTimeout = deviceReconnectionTimeout;
        }


        public float MaxAngleX { get;  }
        public short CyclesCount { get;  }
        public float MovementFrequency { get;  }

        [JsonProperty("UpdateDataPeriod")]
        public TimeSpan UpdateDataPeriod { get;  }

        [JsonProperty("Timeout")]
        public TimeSpan Timeout { get;  }

        /// <inheritdoc />
        [JsonProperty("DeviceReconnectionTimeout")]
        public TimeSpan? DeviceReconnectionTimeout { get; }
    }
}