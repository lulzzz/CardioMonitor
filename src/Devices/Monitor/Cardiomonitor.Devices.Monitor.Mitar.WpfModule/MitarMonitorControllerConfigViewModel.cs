using System;
using System.ComponentModel;
using CardioMonitor.Devices.Monitor;
using CardioMonitor.Devices.WpfModule;
using CardioMonitor.Infrastructure.WpfCommon.Base;

namespace Cardiomonitor.Devices.Monitor.Mitar.WpfModule
{
    public class MitarMonitorControllerConfigViewModel :
        Notifier,
        IDeviceControllerConfigViewModel,
        IDataErrorInfo
    {
        #region Constants

        private const int DefaultUdpPort = 30304;
        private const int DefaultTcpPort = 9761;
        private const int DefaultTimeoutMs = 10000;
        private const int DefaultUpdateDataPeriodMs = 500;

        #endregion

        #region Fields

        private int _monitorBroadcastUdpPort;
        private int _monitorTcpPort;
        private int _updateDataPeriodMs;
        private int _timeoutMs;
        private bool _needReconnect;
        private int _reconnectionTimeoutSec;
        private bool _isDataChanged;

        private readonly MitarMonitorControllerConfigBuilder _configBuilder;

        #endregion

        #region Properties


        public int MonitorBroadcastUdpPort
        {
            get => _monitorBroadcastUdpPort;
            set
            {
                _monitorBroadcastUdpPort = value;
                RisePropertyChanged(nameof(MonitorBroadcastUdpPort));
                RisePropertyChanged(nameof(CanGetConfig));
                IsDataChanged = true;
            }
        }
        public int MonitorTcpPort
        {
            get => _monitorTcpPort;
            set
            {
                _monitorTcpPort = value;
                RisePropertyChanged(nameof(MonitorTcpPort));
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
        
        public MitarMonitorControllerConfigViewModel(MitarMonitorControllerConfigBuilder configBuilder)
        {
            _configBuilder = configBuilder ?? throw new ArgumentNullException(nameof(configBuilder));
            SetDefaults();
        }

        private void SetDefaults()
        {
            NeedReconnect = false;
            UpdateDataPeriodMs = DefaultUpdateDataPeriodMs;
            TimeoutMs = DefaultTimeoutMs;
            MonitorBroadcastUdpPort = DefaultUdpPort;
            MonitorTcpPort = DefaultTcpPort;

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
            var config = new MitarMonitorControlerConfig(
                TimeSpan.FromMilliseconds(UpdateDataPeriodMs),
                TimeSpan.FromMilliseconds(TimeoutMs),
                MonitorBroadcastUdpPort,
                MonitorTcpPort,
                reconnectionTimeout);
            return _configBuilder.Build(config);
        }

        public void SetConfigJson(string jsonConfig)
        {
            if (String.IsNullOrWhiteSpace(jsonConfig))
            {
                SetDefaults();
                return;
            }
            var config = _configBuilder.Build(jsonConfig) as MitarMonitorControlerConfig;
            TimeoutMs = Convert.ToInt32(config.Timeout.TotalMilliseconds);
            UpdateDataPeriodMs = Convert.ToInt32(config.UpdateDataPeriod.TotalMilliseconds);
            NeedReconnect = config.DeviceReconnectionTimeout.HasValue;
            if (config.DeviceReconnectionTimeout.HasValue)
            {
                ReconnectionTimeoutSec = Convert.ToInt32(config.DeviceReconnectionTimeout.Value.TotalSeconds);
            }

            MonitorBroadcastUdpPort = config.MonitorBroadcastUdpPort;
            MonitorTcpPort = config.MonitorTcpPort;
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
                if (String.IsNullOrWhiteSpace(columnName) || Equals(columnName, nameof(MonitorBroadcastUdpPort)))
                {
                    if (MonitorBroadcastUdpPort < 0)
                    {
                        return "Необходимо задать целое положительное число";
                    }
                }
                if (String.IsNullOrWhiteSpace(columnName) || Equals(columnName, nameof(MonitorTcpPort)))
                {
                    if (MonitorTcpPort < 0)
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