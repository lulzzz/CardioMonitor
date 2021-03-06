﻿using System;
using CardioMonitor.Devices.Bed.Infrastructure;
using JetBrains.Annotations;

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
            float frequency, int? deviceReconectionsRetriesCount, TimeSpan? deviceReconnectionTimeout = null)
        {
            BedIpEndpoint = bedIpEndpoint ?? throw new ArgumentNullException(nameof(bedIpEndpoint));
            MaxAngleX = maxAngleX;
            CyclesCount = cyclesCount;
            MovementFrequency = frequency;
            DeviceReconectionsRetriesCount = deviceReconectionsRetriesCount;
            DeviceReconnectionTimeout = deviceReconnectionTimeout;
            Timeout = timeout;
            UpdateDataPeriod = updateDataPeriod;
        }

        /// <summary>
        /// Адрес, по которому можно подключиться к кровати
        /// </summary>
        [NotNull]
        public string BedIpEndpoint { get; }

        /// <summary>
        /// Таймаут операций
        /// </summary>
        public TimeSpan Timeout { get; }

        /// <inheritdoc />
        public TimeSpan? DeviceReconnectionTimeout { get; }

        /// <inheritdoc />
        public int? DeviceReconectionsRetriesCount { get; }

        /// <summary>
        /// Максимальный угол кровати по оси Х, до которой она будет подниматься
        /// </summary>
        public float MaxAngleX { get; }
        
        public short CyclesCount { get; }
        
        public float MovementFrequency { get; }

        /// <summary>
        /// Период обмена сообщениями с устройством
        /// </summary>
        public TimeSpan UpdateDataPeriod { get; }
        
    }
}