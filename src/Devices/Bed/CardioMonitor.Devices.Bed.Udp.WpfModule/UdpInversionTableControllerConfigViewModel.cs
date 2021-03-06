﻿using System;
using System.ComponentModel;
using CardioMonitor.Devices.Bed.UDP;
using CardioMonitor.Devices.WpfModule;
using CardioMonitor.Infrastructure;
using CardioMonitor.Infrastructure.WpfCommon.Base;
using JetBrains.Annotations;

namespace CardioMonitor.Devices.Bed.Udp.WpfModule
{
    public class UdpInversionTableControllerConfigViewModel :
        Notifier,
        IDeviceControllerConfigViewModel,
        IDataErrorInfo
    {
        #region Constants

        private const string DefaultEndpoint = "192.168.0.99:7777";

        private const int DefaultTimeoutMs = 5000;
        private const int DefaultUpdateDataPeriodMs = 900;
        
        #endregion

        #region Fields

        private int _updateDataPeriodMs;
        private int _timeoutMs;
        private bool _needReconnect;
        private int _reconnectionTimeoutSec;
        
        private bool _isDataChanged;

        private readonly BedUdpControllerConfigBuilder _configBuilder;

        private string _endPoint;
        
        private int _deviceReconectionsRetriesCount;

        #endregion

        #region Properties

        public string Endpoint
        {
            get => _endPoint;
            set
            {
                _endPoint = value;
                RisePropertyChanged(nameof(Endpoint));
                RisePropertyChanged(nameof(CanGetConfig));
                IsDataChanged = true;
            }
        }
        
        public int UpdateDataPeriodMs
        {
            get => _updateDataPeriodMs;
            set
            {
                _updateDataPeriodMs = value;
                RisePropertyChanged(nameof(UpdateDataPeriodMs));
                RisePropertyChanged(nameof(CanGetConfig));
                IsDataChanged = true;
            }
        }

        public int TimeoutMs
        {
            get => _timeoutMs;
            set
            {
                _timeoutMs = value;
                RisePropertyChanged(nameof(TimeoutMs));
                RisePropertyChanged(nameof(CanGetConfig));
                IsDataChanged = true;
            }
        }

        public bool NeedReconnect
        {
            get => _needReconnect;
            set
            {
                _needReconnect = value;
                RisePropertyChanged(nameof(NeedReconnect));
                RisePropertyChanged(nameof(CanGetConfig));
                IsDataChanged = true;
            }
        }

        public int ReconnectionTimeoutSec
        {
            get => _reconnectionTimeoutSec;
            set
            {
                _reconnectionTimeoutSec = value;
                RisePropertyChanged(nameof(ReconnectionTimeoutSec));
                RisePropertyChanged(nameof(CanGetConfig));
                IsDataChanged = true;
            }
        }
        
        public int DeviceReconectionsRetriesCount
        {
            get => _deviceReconectionsRetriesCount;
            set
            {
                _deviceReconectionsRetriesCount = value;
                RisePropertyChanged(nameof(DeviceReconectionsRetriesCount));
                RisePropertyChanged(nameof(CanGetConfig));
                IsDataChanged = true;
            }
        }
        
        
        public bool CanGetConfig => String.IsNullOrEmpty(Error);


        public bool IsDataChanged
        {
            get => _isDataChanged;
            private set
            {
                var oldValue = _isDataChanged;
                _isDataChanged = value;
                if (oldValue != value)
                {
                    OnDataChanged?.Invoke(this, EventArgs.Empty);
                }
            }}

        #endregion

        #region Events

        public event EventHandler OnDataChanged;

        #endregion

        public UdpInversionTableControllerConfigViewModel([NotNull] BedUdpControllerConfigBuilder configBuilder)
        {
            _configBuilder = configBuilder ?? throw new ArgumentNullException(nameof(configBuilder));
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            NeedReconnect = false;
            UpdateDataPeriodMs = DefaultUpdateDataPeriodMs;
            TimeoutMs = DefaultTimeoutMs;
            Endpoint = DefaultEndpoint;
        }

        public void ResetDataChanges()
        {
            IsDataChanged = false;
        }


        public string GetConfigJson()
        {
            var reconnectionTimeout = NeedReconnect
                ? TimeSpan.FromSeconds(ReconnectionTimeoutSec)
                : default(TimeSpan?);
            var reconnectionRetriesCount = NeedReconnect
                ? DeviceReconectionsRetriesCount
                : default(int?);
            var config = new BedUdpControllerConfig(
                Endpoint,
                TimeSpan.FromMilliseconds(UpdateDataPeriodMs),
                TimeSpan.FromMilliseconds(TimeoutMs), 
                0,
                0,
                0,
                reconnectionRetriesCount,
                reconnectionTimeout);
            return _configBuilder.Build(config);
        }

        public void SetConfigJson(string jsonConfig)
        {
            if (String.IsNullOrWhiteSpace(jsonConfig))
            {
                SetDefaultValues();
                return;
            }
            var config = _configBuilder.Build(jsonConfig) as BedUdpControllerConfig;
            TimeoutMs = (int)config.Timeout.TotalMilliseconds;
            UpdateDataPeriodMs = (int)config.UpdateDataPeriod.TotalMilliseconds;
            NeedReconnect = config.DeviceReconnectionTimeout.HasValue;
            if (config.DeviceReconnectionTimeout.HasValue)
            {
                ReconnectionTimeoutSec = (int)config.DeviceReconnectionTimeout.Value.TotalSeconds;
            }

            Endpoint = config.BedIpEndpoint;
            DeviceReconectionsRetriesCount = config.DeviceReconectionsRetriesCount ?? 0;


            IsDataChanged = false;
        }

        #region Validation

        public string this[string columnName]
        {
            get
            {
               
                if (String.IsNullOrEmpty(columnName) || Equals(columnName, nameof(Endpoint)))
                {
                    if (!IpEndPointParser.TryParse(Endpoint, out var _))
                    {
                        return "Некорректно задан адрес подключения к инверсионному столу";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || Equals(columnName, nameof(TimeoutMs)))
                {
                    if (TimeoutMs < 100)
                    {
                        return "Должен быть больше 100 мс";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || Equals(columnName, nameof(UpdateDataPeriodMs)))
                {
                    if (UpdateDataPeriodMs <= 300)
                    {
                        return "Должно быть больше 300 мс";
                    }
                }

                if ((String.IsNullOrWhiteSpace(columnName) || Equals(columnName, nameof(ReconnectionTimeoutSec))) &&
                    _needReconnect)
                {
                    if (ReconnectionTimeoutSec < 0)
                    {
                        return "Необходимо задать целое положительное число";
                    }
                }
                
                if ((String.IsNullOrWhiteSpace(columnName) || Equals(columnName, nameof(DeviceReconectionsRetriesCount))) &&
                    _needReconnect)
                {
                    if (DeviceReconectionsRetriesCount < 0)
                    {
                        return "Необходимо задать целое положительное число";
                    }
                }

                return String.Empty;
            }
        }

        public string Error => this[String.Empty];


        #endregion
    }
}