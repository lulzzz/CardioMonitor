using System;
using System.Net;
using CardioMonitor.Devices.Bed.Infrastructure;
using JetBrains.Annotations;

namespace CardioMonitor.Devices.Bed.UDP
{
    /// <summary>
    /// Параметры инициализации контроллера, взаимодействующего с кроватью по протоколу UDP
    /// </summary>
    public class BedUdpControllerInitParams : IBedControllerInitParams
    {
        public BedUdpControllerInitParams(
            [NotNull] IPEndPoint bedIpEndpoint,
            TimeSpan updateDataPeriod,
            TimeSpan timeout,
            double maxAngleX, 
            int cyclesCount, 
            double frequency)
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
        public IPEndPoint BedIPEndpoint { get; }
        
        /// <summary>
        /// Таймаут операций
        /// </summary>
        public TimeSpan Timeout { get; }
        
        /// <inheritdoc />
        public double MaxAngleX { get; }

        /// <inheritdoc />
        public int CyclesCount { get; }

        /// <inheritdoc />
        public double MovementFrequency { get; }

        /// <summary>
        /// Период обмена сообщениями с устройством
        /// </summary>
        public TimeSpan UpdateDataPeriod { get; }
    }
}