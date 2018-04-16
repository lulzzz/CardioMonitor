using System;
using System.ComponentModel;
using CardioMonitor.Devices.WpfModule;
using CardioMonitor.Ui.Base;
using Newtonsoft.Json;

namespace CardioMonitor.Devices.Bed.Fake.WpfModule
{
    public class FadeDeviceControllerConfigViewModel :
        Notifier,
        IDeviceControllerConfigViewModel,
        IDataErrorInfo
    {
        
        public int UpdateDataPeriodMs
        {
            get => _updateDataPeriodMs;
            set
            {
                _updateDataPeriodMs = value;
                RisePropertyChanged(nameof(UpdateDataPeriodMs));
                RisePropertyChanged(nameof(CanGetConfig));
            }
        }

        private const int DefaultTimeoutMs = 10000;
        private const int DefaultUpdateDataPeriodMs = 500;
        private int _updateDataPeriodMs;

        public int TimeoutMs
        {
            get => _timeoutMs;
            set
            {
                _timeoutMs = value;
                RisePropertyChanged(nameof(TimeoutMs));
                RisePropertyChanged(nameof(CanGetConfig));
            }
        }
        private int _timeoutMs;

        public bool CanGetConfig => this[String.Empty] == String.Empty;

        public string GetConfigJson()
        {
            var config = new FakeBedControllerConfig(
                default(float),
                default(short),
                default(float),
                TimeSpan.FromMilliseconds(UpdateDataPeriodMs),
                TimeSpan.FromMilliseconds(TimeoutMs));

            return JsonConvert.SerializeObject(config);
        }

        public void SetConfigJson(string jsonConfig)
        {
            if (String.IsNullOrWhiteSpace(jsonConfig))
            {
                TimeoutMs = DefaultTimeoutMs;
                UpdateDataPeriodMs = DefaultUpdateDataPeriodMs;
                return;
            }
            var config = JsonConvert.DeserializeObject<FakeBedControllerConfig>(jsonConfig);
            TimeoutMs = config.Timeout.Milliseconds;
            UpdateDataPeriodMs = config.UpdateDataPeriod.Milliseconds;
        }

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
                return String.Empty;
            }
        }

        public string Error => this[String.Empty];
    }
}