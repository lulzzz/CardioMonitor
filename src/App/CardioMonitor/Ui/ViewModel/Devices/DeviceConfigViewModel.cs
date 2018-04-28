using System;
using System.Windows;
using CardioMonitor.Devices.WpfModule;
using CardioMonitor.Ui.Base;
using JetBrains.Annotations;

namespace CardioMonitor.Ui.ViewModel.Devices
{
    public class DeviceConfigViewModel : Notifier, IDisposable
    {
        private string _deviceTypeName;
        private string _deviceName;
        private UIElement _configView;
        private IDeviceControllerConfigViewModel _configViewModel;
        private bool _isDataChanged;

        public DeviceConfigViewModel(
            Guid configId,
            [NotNull] string deviceConfigName,
            Guid deviceTypeId,
            [NotNull] string deviceTypeName,
            Guid deviceId,
            [NotNull] string deviceName,
            [NotNull] UIElement configView,
            [NotNull] IDeviceControllerConfigViewModel configViewModel)
        {
            _deviceTypeName = deviceTypeName ?? throw new ArgumentNullException(nameof(deviceTypeName));
            _deviceName = deviceName ?? throw new ArgumentNullException(nameof(deviceName));
            _deviceConfigName = deviceConfigName ?? throw new ArgumentNullException(nameof(deviceConfigName));
            _configView = configView ?? throw new ArgumentNullException(nameof(configView));
            _configViewModel = configViewModel ?? throw new ArgumentNullException(nameof(configViewModel));
            ConfigId = configId;
            DeviceTypeId = deviceTypeId;
            DeviceId = deviceId;
            _configViewModel.OnDataChanged += ConfigViewModelOnOnDataChanged;
        }

        private void ConfigViewModelOnOnDataChanged(object sender, EventArgs eventArgs)
        {
            OnDataChanged?.Invoke(sender, eventArgs);
        }

        public event EventHandler OnDataChanged;

        public Guid ConfigId { get; }
        public Guid DeviceTypeId { get; }
        public Guid DeviceId { get; }

        public string DeviceTypeName
        {
            get => _deviceTypeName;
            set
            {
                _deviceTypeName = value;
                RisePropertyChanged(nameof(DeviceTypeName));
            }
        }

        public string DeviceName
        {
            get => _deviceName;
            set
            {
                _deviceName = value;
                RisePropertyChanged(nameof(DeviceName));
            }
        }

        public string DeviceConfigName
        {
            get => _deviceConfigName;
            set
            {
                _deviceConfigName = value;
                RisePropertyChanged(nameof(DeviceConfigName));
                IsDataChanged = true;
            }
        }

        private string _deviceConfigName;

        public UIElement ConfigView
        {
            get => _configView;
            set
            {
                _configView = value;
                RisePropertyChanged(nameof(ConfigView));
            }
        }


        public IDeviceControllerConfigViewModel ConfigViewModel
        {
            get => _configViewModel;
            set
            {
                _configViewModel = value;
                RisePropertyChanged(nameof(ConfigViewModel));
            }
        }

        public bool IsDataChanged
        {
            get => _isDataChanged;
            set
            {
                _isDataChanged = value;
                RisePropertyChanged(nameof(IsDataChanged));
                OnDataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ResetDataChanges()
        {
            IsDataChanged = false;
            ConfigViewModel?.ResetDataChanges();
        }

        public void Dispose()
        {
            if (ConfigViewModel != null)
            {
                ConfigViewModel.OnDataChanged -= ConfigViewModelOnOnDataChanged;
            }
        }
    }

}