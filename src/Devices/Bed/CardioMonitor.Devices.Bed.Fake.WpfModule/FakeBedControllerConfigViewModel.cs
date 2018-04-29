using System;
using System.ComponentModel;
using CardioMonitor.Devices.Bed.Fake.WpfModule.Annotations;
using CardioMonitor.Devices.WpfModule;
using CardioMonitor.Ui.Base;
using Newtonsoft.Json;

namespace CardioMonitor.Devices.Bed.Fake.WpfModule
{
    public class FakeBedControllerConfigViewModel :
        Notifier,
        IDeviceControllerConfigViewModel,
        IDataErrorInfo
    {
        #region Constants

        private const int DefaultTimeoutMs = 10000;
        private const int DefaultUpdateDataPeriodMs = 500;

        private const int DefaultConnectDelayMs = 1000;
        private const int DefaultDisconnectDelayMs = 1000;
        private const int DefaultDelayMs = 1000;
        private const int DefaultCycleWithMaxAngelDurationSec = 600;

        #endregion

        #region Fields

        private int _updateDataPeriodMs;
        private int _timeoutMs;
        private bool _needReconnect;
        private int _reconnectionTimeoutSec;
        
        private int _connectDelayMs;
        private int _disconnectDelayMs;
        private int _delayMs;
        private int _cycleWithMaxAngelDurationSec;

        private bool _isDataChanged;

        private readonly FakeBedControllerConfigBuilder _configBuilder;

        #endregion

        #region Properties

        public int ConnectDelayMs
        {
            get => _connectDelayMs;
            set
            {
                _connectDelayMs = value;
                RisePropertyChanged(nameof(ConnectDelayMs));
                RisePropertyChanged(nameof(CanGetConfig));
                IsDataChanged = true;
            }
        }

        public int DisconnectDelayMs
        {
            get => _disconnectDelayMs;
            set
            {
                _disconnectDelayMs = value;
                RisePropertyChanged(nameof(DisconnectDelayMs));
                RisePropertyChanged(nameof(CanGetConfig));
                IsDataChanged = true;
            }
        }

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

        public int CycleWithMaxAngelDurationSec
        {
            get => _cycleWithMaxAngelDurationSec;
            set
            {
                _cycleWithMaxAngelDurationSec = value;
                RisePropertyChanged(nameof(CycleWithMaxAngelDurationSec));
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
                    OnDataChanged?.Invoke(this, EventArgs.Empty);
                }
            }}

        #endregion

        #region Events

        public event EventHandler OnDataChanged;

        #endregion

        public FakeBedControllerConfigViewModel([NotNull] FakeBedControllerConfigBuilder configBuilder)
        {
            _configBuilder = configBuilder ?? throw new ArgumentNullException(nameof(configBuilder));
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            NeedReconnect = false;
            UpdateDataPeriodMs = DefaultUpdateDataPeriodMs;
            TimeoutMs = DefaultTimeoutMs;
            DelayMs = DefaultDelayMs;
            ConnectDelayMs = DefaultConnectDelayMs;
            DisconnectDelayMs = DefaultDisconnectDelayMs;
            CycleWithMaxAngelDurationSec = DefaultCycleWithMaxAngelDurationSec;
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
            var config = new FakeBedControllerConfig(
                default(float),
                default(short),
                default(float),
                TimeSpan.FromMilliseconds(UpdateDataPeriodMs),
                TimeSpan.FromMilliseconds(TimeoutMs), 
                TimeSpan.FromMilliseconds(ConnectDelayMs),
                TimeSpan.FromMilliseconds(DisconnectDelayMs),
                TimeSpan.FromMilliseconds(DelayMs),
                TimeSpan.FromSeconds(CycleWithMaxAngelDurationSec),
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
            var config = _configBuilder.Build(jsonConfig) as FakeBedControllerConfig;
            TimeoutMs = (int)config.Timeout.TotalMilliseconds;
            UpdateDataPeriodMs = (int)config.UpdateDataPeriod.TotalMilliseconds;
            NeedReconnect = config.DeviceReconnectionTimeout.HasValue;
            if (config.DeviceReconnectionTimeout.HasValue)
            {
                ReconnectionTimeoutSec = (int)config.DeviceReconnectionTimeout.Value.TotalSeconds;
            }

            DelayMs = (int)config.DefaultDelay.TotalMilliseconds;
            ConnectDelayMs = (int)config.ConnectDelay.TotalMilliseconds;
            DisconnectDelayMs = (int)config.DisconnectDelay.TotalMilliseconds;
            CycleWithMaxAngelDurationSec = (int)config.CycleWithMaxAngleDuration.TotalSeconds;


            IsDataChanged = false;
        }

        #region Validation

        public string this[string columnName]
        {
            get
            {
                if (String.IsNullOrEmpty(columnName) || Equals(columnName, nameof(DelayMs)))
                {
                    if (DelayMs < 0)
                    {
                        return "Необходимо задать целое положительное число";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || Equals(columnName, nameof(DisconnectDelayMs)))
                {
                    if (DisconnectDelayMs < 0)
                    {
                        return "Необходимо задать целое положительное число";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || Equals(columnName, nameof(ConnectDelayMs)))
                {
                    if (ConnectDelayMs < 0)
                    {
                        return "Необходимо задать целое положительное число";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || Equals(columnName, nameof(CycleWithMaxAngelDurationSec)))
                {
                    if (CycleWithMaxAngelDurationSec < 180)
                    {
                        return "Длительность эмуляции не может быть меньше 3 минут";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || Equals(columnName, nameof(TimeoutMs)))
                {
                    if (TimeoutMs < 100)
                    {
                        return "Должен быть не меньше 100 мс";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || Equals(columnName, nameof(UpdateDataPeriodMs)))
                {
                    if (UpdateDataPeriodMs <= 300)
                    {
                        return "Должно быть не меньше 300 мс";
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

                return String.Empty;
            }
        }

        public string Error => this[String.Empty];


        #endregion
    }
}