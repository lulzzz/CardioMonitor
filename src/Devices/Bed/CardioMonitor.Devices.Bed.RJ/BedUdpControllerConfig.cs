using System;
using System.Net;
using CardioMonitor.Devices.Bed.Infrastructure;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CardioMonitor.Devices.Bed.UDP
{
    /// <summary>
    /// Параметры инициализации контроллера, взаимодействующего с кроватью по протоколу UDP
    /// </summary>
    public class BedUdpControllerConfig : IBedControllerConfig
    {
        public BedUdpControllerConfig(
            [NotNull] string bedIpEndpoint,
            TimeSpan updateDataPeriod,
            TimeSpan timeout,
            float maxAngleX, 
            short cyclesCount, 
            float frequency)
        {
            BedIPEndpoint = bedIpEndpoint ?? throw new ArgumentNullException(nameof(bedIpEndpoint));
            MaxAngleX = maxAngleX;
            CyclesCount = cyclesCount;
            MovementFrequency = frequency;
            Timeout = timeout;
            UpdateDataPeriod = updateDataPeriod;
        }

        /// <summary>
        /// Адрес, по которому можно подключиться к кровати
        /// </summary>
        [NotNull]
        [JsonProperty("BedIPEndpoint")]
        public string BedIPEndpoint { get; }

        /// <summary>
        /// Таймаут операций
        /// </summary>
        [JsonProperty("Timeout")]
        public TimeSpan Timeout { get; }

        /// <inheritdoc />
        [JsonProperty("MaxAngleX")]
        public float MaxAngleX { get; }

        /// <inheritdoc />
        [JsonProperty("CyclesCount")]
        public short CyclesCount { get; }

        /// <inheritdoc />
        [JsonProperty("MovementFrequency")]
        public float MovementFrequency { get; }

        /// <summary>
        /// Период обмена сообщениями с устройством
        /// </summary>
        [JsonProperty("UpdateDataPeriod")]
        public TimeSpan UpdateDataPeriod { get; }
    }
}