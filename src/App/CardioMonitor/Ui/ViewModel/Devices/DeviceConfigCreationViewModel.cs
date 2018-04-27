using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.Devices;
using CardioMonitor.Ui.Base;
using JetBrains.Annotations;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.ViewModel.Devices
{
    public class DeviceConfigCreationViewModel : 
        Notifier, 
        IStoryboardPageViewModel,
        IDataErrorInfo
    {
        #region Fields

        [NotNull]
        private readonly IDeviceModulesController _deviceModulesController;

        private ObservableCollection<DeviceInfoElement> _deviceTypes;

        private ObservableCollection<DeviceInfoElement> _devices;

        private DeviceInfoElement _selectedDeviceType;

        private DeviceInfoElement _selectedDevice;

        private string _configName;

        private ICommand _saveCommand;

        #endregion

        public DeviceConfigCreationViewModel([NotNull] IDeviceModulesController deviceModulesController)
        {
            _deviceModulesController = deviceModulesController ?? throw new ArgumentNullException(nameof(deviceModulesController));
        }

        #region Properties

        public ObservableCollection<DeviceInfoElement> DeviceTypes
        {
            get => _deviceTypes;
            set
            {
                _deviceTypes = value; 
                RisePropertyChanged(nameof(DeviceTypes));

                SelectedDevice = null;
                Devices = new ObservableCollection<DeviceInfoElement>();
            }
        }


        public ObservableCollection<DeviceInfoElement> Devices
        {
            get => _devices;
            set
            {
                _devices = value; 
                RisePropertyChanged(nameof(Devices));

                var selectedDeviceId = SelectedDevice?.Id;
                if (!selectedDeviceId.HasValue) return;

                SelectedDevice = _devices.FirstOrDefault(x => x.Id == selectedDeviceId.Value);
            }
        }

        public DeviceInfoElement SelectedDeviceType
        {
            get => _selectedDeviceType;
            set
            {
                if (Equals(_selectedDeviceType, value)) return;
                _selectedDeviceType = value;
                RisePropertyChanged(nameof(SelectedDeviceType));
                RisePropertyChanged(nameof(SaveCommand));
                if (_selectedDeviceType == null) return;

                var devices = _deviceModulesController.GetRegisteredDevices(_selectedDeviceType.Id);

                var devicesInfos = new List<DeviceInfoElement>(devices.Count);
                foreach (var deviceInfo in devices)
                {
                    devicesInfos.Add(new DeviceInfoElement
                    {
                        Id = deviceInfo.DeviceId,
                        Name = deviceInfo.Name
                    });
                }
                Devices = new ObservableCollection<DeviceInfoElement>(devicesInfos);
            }
        }

        public DeviceInfoElement SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value; 
                RisePropertyChanged(nameof(SelectedDevice));
                RisePropertyChanged(nameof(SaveCommand));
            }
        }

        public string ConfigName
        {
            get => _configName;
            set
            {
                _configName = value; 
                RisePropertyChanged(nameof(ConfigName));
                RisePropertyChanged(nameof(SaveCommand));
            }
        }

        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new SimpleCommand
            {
                CanExecuteDelegate = x => IsValid,
                ExecuteDelegate = async x => await SaveAsync().ConfigureAwait(true)
            }); }
        }

        #endregion

        private Task SaveAsync()
        {

            return PageCompleted?.InvokeAsync(
                this,
                new DeviceConfigsViewModelPageContext
                {
                    ConfigName = ConfigName,
                    DeviceId = SelectedDevice.Id,
                    DeviceTypeId = SelectedDeviceType.Id,
                    IsAdded = false
                });
        }

        #region IStoryboardViewModel

        public void Dispose()
        {
        }

        public Guid PageId { get; set; }
        public Guid StoryboardId { get; set; }
        public Task OpenAsync(IStoryboardPageContext context)
        {
            var deviceTypes = _deviceModulesController.GetRegisteredDevicesTypes();
            var result = new List<DeviceInfoElement>(deviceTypes.Count);
            foreach (var deviceTypeInfo in deviceTypes)
            {
                result.Add(new DeviceInfoElement
                {
                    Id = deviceTypeInfo.DeviceTypeId,
                    Name = deviceTypeInfo.Name
                });
            }

            DeviceTypes = new ObservableCollection<DeviceInfoElement>(result);

            return Task.CompletedTask;
        }

        public Task<bool> CanLeaveAsync()
        {
            return Task.FromResult(true);
        }

        public Task LeaveAsync()
        {
            return Task.CompletedTask;
        }

        public Task ReturnAsync(IStoryboardPageContext context)
        {
            return Task.CompletedTask;
        }

        public Task<bool> CanCloseAsync()
        {
            return Task.FromResult(true);
        }

        public Task CloseAsync()
        {
            return Task.CompletedTask;
        }

        public event Func<object, Task> PageCanceled;
        public event Func<object, Task> PageCompleted;
        public event Func<object, Task> PageBackRequested;
        public event Func<object, TransitionRequest, Task> PageTransitionRequested;

        #endregion

        #region Validation

        public string this[string columnName]
        {
            get
            {
                if (String.IsNullOrEmpty(columnName) || Equals(columnName, nameof(SelectedDeviceType)))
                {
                    if (SelectedDeviceType == null)
                    {
                        return "Выберити тип устройства";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || Equals(columnName, nameof(SelectedDevice)))
                {
                    if (SelectedDevice == null)
                    {
                        return "Выберити устройство";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || Equals(columnName, nameof(ConfigName)))
                {
                    if (SelectedDeviceType == null)
                    {
                        return "Введите название конфигурации";
                    }
                }
                return String.Empty;
            }
        }

        public string Error => this[String.Empty];

        public bool IsValid => String.IsNullOrEmpty(Error);


        #endregion

    }

    public class DeviceInfoElement
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
