﻿using System;
using System.ComponentModel;
using CardioMonitor.Devices.WpfModule;
using CardioMonitor.Ui.Base;
using Newtonsoft.Json;

namespace CardioMonitor.Devices.Monitor.Fake.WpfModule
{
    public class FakeMonitorControllerConfigViewModel :
        Notifier,
        IDeviceControllerConfigViewModel,
        IDataErrorInfo
    {
        #region Constants

        private const int DefaultDelayMs = 1000;
        private const int DefaultPumpingDelayMs = 1500;
        private const int DefaultTimeoutMs = 10000;
        private const int DefaultUpdateDataPeriodMs = 500;

        #endregion

        #region Fields

        private int _delayMs;
        private int _pumpingDelayMs;
        private int _updateDataPeriodMs;
        private int _timeoutMs;
        private bool _needReconnect;
        private int _reconnectionTimeoutSec;
        private bool _isDataChanged;

        #endregion

        #region Properties

        public int DelayMs
        {
            get => _delayMs;
            set
            {
                _delayMs = value;
                RisePropertyChanged(nameof(DelayMs));
                RisePropertyChanged(nameof(CanGetConfig));
                IsDataChanged = true;
            }
        }
        public int PumpingDelayMs
        {
            get => _pumpingDelayMs;
            set
            {
                _pumpingDelayMs = value;
                RisePropertyChanged(nameof(PumpingDelayMs));
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
                    CanSaveChanged?.Invoke(this, EventArgs.Empty);
                }
            }}

        #endregion

        #region Events

        public event EventHandler CanSaveChanged;

        #endregion

        public FakeMonitorControllerConfigViewModel()
        {
            NeedReconnect = false;
            UpdateDataPeriodMs = DefaultUpdateDataPeriodMs;
            TimeoutMs = DefaultTimeoutMs;
            DelayMs = DefaultDelayMs;
            PumpingDelayMs = DefaultPumpingDelayMs;
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
            var config = new FakeCardioMonitorConfig(
                TimeSpan.FromMilliseconds(UpdateDataPeriodMs),
                TimeSpan.FromMilliseconds(TimeoutMs),
                TimeSpan.FromMilliseconds(DelayMs),
                TimeSpan.FromMilliseconds(PumpingDelayMs),
                reconnectionTimeout);
            return JsonConvert.SerializeObject(config);
        }

        public void SetConfigJson(string jsonConfig)
        {
            if (String.IsNullOrWhiteSpace(jsonConfig))
            {
                TimeoutMs = DefaultTimeoutMs;
                UpdateDataPeriodMs = DefaultUpdateDataPeriodMs;
                NeedReconnect = false;
                return;
            }
            var config = JsonConvert.DeserializeObject<FakeCardioMonitorConfig>(jsonConfig);
            TimeoutMs = config.Timeout.Milliseconds;
            UpdateDataPeriodMs = config.UpdateDataPeriod.Milliseconds;
            NeedReconnect = config.DeviceReconnectionTimeout.HasValue;
            if (config.DeviceReconnectionTimeout.HasValue)
            {
                ReconnectionTimeoutSec = config.DeviceReconnectionTimeout.Value.Seconds;
            }

            IsDataChanged = false;
        }

        #region Validation

        public string this[string columnName]
        {
            get
            {
                if (String.IsNullOrEmpty(columnName) || Equals(columnName, nameof(TimeoutMs)))
                {
                    if (TimeoutMs < 0)
                    {
                        return "Необходимо задать целое положительное число";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || Equals(columnName, nameof(UpdateDataPeriodMs)))
                {
                    if (UpdateDataPeriodMs < 0)
                    {
                        return "Необходимо задать целое положительное число";
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
                if (String.IsNullOrWhiteSpace(columnName) || Equals(columnName, nameof(DelayMs)))
                {
                    if (DelayMs < 0)
                    {
                        return "Необходимо задать целое положительное число";
                    }
                }
                if (String.IsNullOrWhiteSpace(columnName) || Equals(columnName, nameof(PumpingDelayMs)))
                {
                    if (PumpingDelayMs < 0)
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