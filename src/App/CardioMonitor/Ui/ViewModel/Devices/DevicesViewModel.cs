using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Configuration;
using CardioMonitor.Devices.WpfModule;
using CardioMonitor.Ui.Base;
using JetBrains.Annotations;
using Markeli.Storyboards;
using Markeli.Utils.Logging;
using DeviceTypeInfo = CardioMonitor.Devices.DeviceTypeInfo;

namespace CardioMonitor.Ui.ViewModel.Devices
{
    public class DeviceConfigViewModel: Notifier
    {
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
        }

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
        private string _deviceTypeName;

        public string DeviceName
        {
            get => _deviceName;
            set
            {
                _deviceName = value;
                RisePropertyChanged(nameof(DeviceName));
            }
        }
        private string _deviceName;

        public string DeviceConfigName
        {
            get => _deviceConfigName;
            set
            {
                _deviceConfigName = value;
                RisePropertyChanged(nameof(DeviceConfigName));
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
        private UIElement _configView;


        public IDeviceControllerConfigViewModel ConfigViewModel
        {
            get => _configViewModel; set
            {
                _configViewModel = value;
                RisePropertyChanged(nameof(ConfigViewModel));
            }
        }
        private IDeviceControllerConfigViewModel _configViewModel;
    }

    public class DevicesViewModel: Notifier, IStoryboardPageViewModel
    {
        private readonly ILogger _logger;


        private ObservableCollection<DeviceConfigViewModel> _deviceConfigs;

        private DeviceConfigViewModel _selectedDeviceConfig;

        private readonly IDeviceConfigurationService _configurationService;

        private readonly IDeviceModulesController _deviceModulesController;

        private readonly Dictionary<Guid, DeviceTypeInfo> _deviceTypes;

        private readonly Dictionary<Guid, DeviceInfo> _deviceInfos;

        public DevicesViewModel(
            [NotNull] ILogger logger,
            [NotNull] IDeviceConfigurationService configurationService,
            [NotNull] IDeviceModulesController deviceModulesController)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _deviceModulesController = deviceModulesController ?? throw new ArgumentNullException(nameof(deviceModulesController));

            _deviceTypes = new Dictionary<Guid, DeviceTypeInfo>();
            _deviceInfos = new Dictionary<Guid, DeviceInfo>();
        }


        private ICommand _addCommand;
        private ICommand _removeCommand;
        private ICommand _saveCommand;

        public ObservableCollection<DeviceConfigViewModel> DeviceConfigs
        {
            get => _deviceConfigs;
            set { _deviceConfigs = value;
                RisePropertyChanged(nameof(DeviceConfigs));
            }
        }

        public DeviceConfigViewModel SelectedDeviceConfig
        {
            get => _selectedDeviceConfig;
            set
            {
                _selectedDeviceConfig = value;
                RisePropertyChanged(nameof(SelectedDeviceConfig));
            }
        }


        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                RisePropertyChanged(nameof(IsBusy));
            }
        }
        private bool _isBusy;

        public string BusyMessage
        {
            get => _busyMessage;
            set
            {
                _busyMessage = value;
                RisePropertyChanged(nameof(BusyMessage));
            }
        }
        private string _busyMessage;

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => SelectedDeviceConfig?.ConfigViewModel != null && SelectedDeviceConfig.ConfigViewModel.CanGetConfig,
                    ExecuteDelegate = async x => await SaveConfigAsync().ConfigureAwait(true)
                });
            }
        }

        public ICommand AddCommand
        {
            get
            {
                return _addCommand ?? (_addCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = async x => await AddConfigAsync().ConfigureAwait(true)
                });
            }
        }
        public ICommand RemoveCommand
        {
            get
            {
                return _removeCommand ?? (_removeCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => SelectedDeviceConfig != null,
                    ExecuteDelegate = async x => await RemoveConfigAsync().ConfigureAwait(true)
                });
            }
        }

        private async Task SaveConfigAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Сохранение изменений...";
                var selectedDeviceConfig = SelectedDeviceConfig;
                var jsonConfig = selectedDeviceConfig.ConfigViewModel.GetConfigJson();

                var deviceConfiguration = new DeviceConfiguration
                {
                    ConfigId = selectedDeviceConfig.ConfigId,
                    ConfigName = selectedDeviceConfig.DeviceConfigName,
                    DeviceId = selectedDeviceConfig.DeviceId,
                    DeviceTypeId = selectedDeviceConfig.DeviceTypeId,
                    ParamsJson = jsonConfig
                };

                await _configurationService
                    .EditDeviceConfigurationAsync(deviceConfiguration)
                    .ConfigureAwait(true);
            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: ошибка сохранения изменений конфигурации устройства. Причина: {e.Message}", e);
                await MessageHelper.Instance.ShowMessageAsync("Ошибка сохранения изменений конфигурации").ConfigureAwait(false);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddConfigAsync()
        {
            var handler = PageTransitionRequested;
            if (handler == null) return;
            await handler.Invoke(
                this,
                new TransitionRequest(PageIds.DeviceCreationPageId, null))
                .ConfigureAwait(true);
        }

        private async Task RemoveConfigAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Удаление конфигурации...";

                await _configurationService
                    .DeleteDeviceConfigurationAsync(SelectedDeviceConfig.ConfigId)
                    .ConfigureAwait(true);

                DeviceConfigs.Remove(SelectedDeviceConfig);
                SelectedDeviceConfig = null;
            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: ошибка удаления конфигурации. Причина: {e.Message}", e);
                await MessageHelper.Instance.ShowMessageAsync("Ошибка удаления конфигурации");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task InitPageAsync() {
            try
            {
                IsBusy = true;
                BusyMessage = "Загрузка конфигураций";
                var deviceTypes = _deviceModulesController.GetRegisteredDevicesTypes();
                foreach (var deviceTypeInfo in deviceTypes)
                {
                    _deviceTypes[deviceTypeInfo.DeviceTypeId] = deviceTypeInfo;

                    var deviceInfos = _deviceModulesController.GetRegisteredDevices(deviceTypeInfo.DeviceTypeId);
                    foreach (var deviceInfo in deviceInfos)
                    {
                        _deviceInfos[deviceInfo.DeviceId] = deviceInfo;

                    }
                }

                var savedConfigs = await _configurationService
                    .GetConfigurationsAsync()
                    .ConfigureAwait(true);
                savedConfigs = savedConfigs ?? new List<DeviceConfiguration>(0);

                var deviceConfigs = new List<DeviceConfigViewModel>(savedConfigs.Count);
                foreach (var savedConfig in savedConfigs)
                {
                    if (!_deviceTypes.ContainsKey(savedConfig.DeviceTypeId))
                    {
                        _logger.Warning(
                            $"{GetType().Name}: Не зарегистрирован тип устройства {savedConfig.DeviceTypeId}");
                        continue;
                    }

                    if (!_deviceInfos.ContainsKey(savedConfig.DeviceId))
                    {
                        _logger.Warning($"{GetType().Name}: Не зарегистрировано устройство {savedConfig.DeviceId}");
                        continue;
                    }

                    var view = _deviceModulesController.GetView(savedConfig.DeviceId);
                    var viewModel = _deviceModulesController.GetViewModel(savedConfig.DeviceId);
                    viewModel.SetConfigJson(savedConfig.ParamsJson);
                    view.DataContext = viewModel;

                    var deviceViewModel = new DeviceConfigViewModel(
                        savedConfig.ConfigId,
                        savedConfig.ConfigName,
                        savedConfig.DeviceTypeId,
                        _deviceTypes[savedConfig.DeviceTypeId].Name,
                        savedConfig.DeviceId,
                        _deviceInfos[savedConfig.DeviceId].Name,
                        view,
                        viewModel);
                    deviceConfigs.Add(deviceViewModel);
                }

                DeviceConfigs = new ObservableCollection<DeviceConfigViewModel>(deviceConfigs);

            }
            catch (Exception e)
            {
                await MessageHelper.Instance.ShowMessageAsync("Ошибка загрузки списка конфигураций").ConfigureAwait(true);
                _logger.Error($"{GetType().Name}: ошибка загрузки списка конфигураций. Причина: {e.Message}", e);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddConfigWithDefaultValuesAsync([NotNull] DevicesViewModelPageContext context)
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Добавление новой конфигурации со значениями по умолчанию...";
                context.IsAdded = true;

                var viewModel = _deviceModulesController.GetViewModel(context.DeviceId);

                var defaultJsonConfig = viewModel.GetConfigJson();

                var deviceConfig = new DeviceConfiguration
                {
                    ConfigId = Guid.NewGuid(),
                    ConfigName = context.ConfigName,
                    ParamsJson = defaultJsonConfig,
                    DeviceId = context.DeviceId,
                    DeviceTypeId = context.DeviceTypeId
                };

                await _configurationService
                    .AddDeviceConfigurationAsync(deviceConfig)
                    .ConfigureAwait(false);

                var view = _deviceModulesController.GetView(context.DeviceId);
                view.DataContext = viewModel;

                var deviceViewModel = new DeviceConfigViewModel(
                    deviceConfig.ConfigId,
                    deviceConfig.ConfigName, 
                    deviceConfig.DeviceTypeId,
                    _deviceTypes[deviceConfig.DeviceTypeId].Name,
                    deviceConfig.DeviceId,
                    _deviceInfos[deviceConfig.DeviceId].Name,
                    view,
                    viewModel);

                DeviceConfigs.Add(deviceViewModel);

                SelectedDeviceConfig = deviceViewModel;

            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: Ошибка добавления нового конфига. Причина: {e.Message}", e);
                await MessageHelper.Instance.ShowMessageAsync("Ошибка добавления нового конфига")
                    .ConfigureAwait(true);
            }
            finally
            {
                IsBusy = false;
            }

        }

        #region IStoryboardPageViewModel

        public void Dispose()
        {
        }

        public Guid PageId { get; set; }
        public Guid StoryboardId { get; set; }

        public Task OpenAsync(IStoryboardPageContext context)
        {
            Task.Factory.StartNew(async () => await InitPageAsync().ConfigureAwait(false)).ConfigureAwait(false);
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
            if (!(context is DevicesViewModelPageContext localContext)) return Task.CompletedTask;
            if (localContext.IsAdded) return Task.CompletedTask;

            Task.Factory.StartNew(async () => 
                    await AddConfigWithDefaultValuesAsync(localContext)
                        .ConfigureAwait(false))
                .ConfigureAwait(false);

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
    }

    public class DevicesViewModelPageContext : IStoryboardPageContext
    {
        public bool IsAdded { get; set; }

        public Guid DeviceId { get; set; }

        public Guid DeviceTypeId { get; set; }

        public string ConfigName { get; set; }
    }
}