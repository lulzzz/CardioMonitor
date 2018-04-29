using System;
using CardioMonitor.Devices.Bed.Infrastructure;

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
            TimeSpan connectDelay,
            TimeSpan disconnectDelay,
            TimeSpan defaultdelay,
            TimeSpan cyclesWithMaxAngleDuration,
            TimeSpan? deviceReconnectionTimeout = null)
        {
            MaxAngleX = maxAngleX;
            CyclesCount = cyclesCount;
            MovementFrequency = movementFrequency;
            UpdateDataPeriod = updateDataPeriod;
            Timeout = timeout;
            ConnectDelay = connectDelay;
            DisconnectDelay = disconnectDelay;
            DefaultDelay = defaultdelay;
            CycleWithMaxAngleDuration = cyclesWithMaxAngleDuration;
            DeviceReconnectionTimeout = deviceReconnectionTimeout;
        }


        public float MaxAngleX { get;  }
        public short CyclesCount { get;  }
        public float MovementFrequency { get;  }
        
        public TimeSpan UpdateDataPeriod { get;  }
        
        public TimeSpan Timeout { get;  }

        /// <inheritdoc />
        public TimeSpan? DeviceReconnectionTimeout { get; }
        
        public TimeSpan ConnectDelay { get; }
        
        public TimeSpan DisconnectDelay { get; }
        
        public TimeSpan DefaultDelay { get;  }
        
        public TimeSpan CycleWithMaxAngleDuration { get; }
    }
}