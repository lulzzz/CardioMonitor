using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Configuration;
using CardioMonitor.Events.Devices;
using CardioMonitor.Ui.Base;
using JetBrains.Annotations;
using Markeli.Storyboards;
using Markeli.Utils.EventBus.Contracts;
using Markeli.Utils.Logging;
using ToastNotifications.Messages;
using DeviceTypeInfo = CardioMonitor.Devices.DeviceTypeInfo;

namespace CardioMonitor.Ui.ViewModel.Devices
{
    public class DeviceConfigsViewModel: Notifier, IStoryboardPageViewModel
    {
        #region Fields
        
        [NotNull]
        private readonly ILogger _logger;

        [NotNull] private readonly IUiInvoker _uiInvoker;

        private ICommand _addCommand;
        private ICommand _removeCommand;
        private ICommand _saveCommand;

        private ObservableCollection<DeviceConfigViewModel> _deviceConfigs;

        private DeviceConfigViewModel _selectedDeviceConfig;

        [NotNull]
        private readonly IDeviceConfigurationService _configurationService;

        [NotNull]
        private readonly IDeviceModulesController _deviceModulesController;

        [NotNull]
        private readonly Dictionary<Guid, DeviceTypeInfo> _deviceTypes;

        [NotNull]
        private readonly Dictionary<Guid, DeviceInfo> _deviceInfos;

        [NotNull]
        private readonly ToastNotifications.Notifier _notifier;

        [NotNull]
        private readonly IEventBus _eventBus;

        private string _busyMessage;
        private bool _isBusy;
        
        #endregion


        public DeviceConfigsViewModel(
            [NotNull] ILogger logger,
            [NotNull] IDeviceConfigurationService configurationService,
            [NotNull] IDeviceModulesController deviceModulesController, [NotNull] IUiInvoker uiInvoker,
            [NotNull] ToastNotifications.Notifier notifier, 
            [NotNull] IEventBus eventBus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _deviceModulesController = deviceModulesController ?? throw new ArgumentNullException(nameof(deviceModulesController));
            _uiInvoker = uiInvoker;
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _deviceTypes = new Dictionary<Guid, DeviceTypeInfo>();
            _deviceInfos = new Dictionary<Guid, DeviceInfo>();
        }
        

        #region Properties

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
                if (_selectedDeviceConfig?.ConfigViewModel != null)
                {
                    _selectedDeviceConfig.OnDataChanged -= ValidateOnDataCommand;
                }
                _selectedDeviceConfig = value;
                if (_selectedDeviceConfig?.ConfigViewModel != null)
                {
                    _selectedDeviceConfig.OnDataChanged += ValidateOnDataCommand;
                }
                RisePropertyChanged(nameof(SelectedDeviceConfig));
                RisePropertyChanged(nameof(SaveCommand));
                RisePropertyChanged(nameof(RemoveCommand));
                RisePropertyChanged(nameof(IsConfigSelected));
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

        public string BusyMessage
        {
            get => _busyMessage;
            set
            {
                _busyMessage = value;
                RisePropertyChanged(nameof(BusyMessage));
            }
        }

        public bool IsConfigSelected => SelectedDeviceConfig != null;

        #endregion

        #region Commands


        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => SelectedDeviceConfig?.ConfigViewModel != null 
                                              && (SelectedDeviceConfig.ConfigViewModel.IsDataChanged || SelectedDeviceConfig.IsDataChanged)
                                              && SelectedDeviceConfig.ConfigViewModel.CanGetConfig,
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

        #endregion

        private void ValidateOnDataCommand(object sender, EventArgs args)
        {
            RisePropertyChanged(nameof(SaveCommand));
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
                selectedDeviceConfig.ResetDataChanges();
                _notifier.ShowSuccess("Изменения в конфигурации сохранены");
            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: ошибка сохранения изменений конфигурации устройства. Причина: {e.Message}", e);
                _notifier.ShowError("Ошибка сохранения изменений конфигурации");
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
                _notifier.ShowSuccess("Конфигурация удалена");
            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: ошибка удаления конфигурации. Причина: {e.Message}", e);
                _notifier.ShowSuccess("Ошибка удаления конфигурации");
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
                BusyMessage = "Загрузка конфигураций...";
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
                    _uiInvoker.Invoke(() =>
                    {

                        view.DataContext = viewModel;
                    });

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
                _notifier.ShowError("Ошибка загрузки списка конфигураций");
                _logger.Error($"{GetType().Name}: ошибка загрузки списка конфигураций. Причина: {e.Message}", e);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddConfigWithDefaultValuesAsync([NotNull] DeviceConfigsViewModelPageContext context)
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

                var configId = await _configurationService
                    .AddDeviceConfigurationAsync(deviceConfig)
                    .ConfigureAwait(false);
                
                _uiInvoker.Invoke(() =>
                {
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
                });
;
                _notifier.ShowSuccess("Новая конфигурация устройства добавлена");
            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: Ошибка добавления нового конфига. Причина: {e.Message}", e);
                _notifier.ShowError("Ошибка добавления нового конфига");
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
            if (!(context is DeviceConfigsViewModelPageContext localContext)) return Task.CompletedTask;
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

        public event Func<TransitionEvent, Task> PageCanceled;
        public event Func<TransitionEvent, Task> PageCompleted;
        public event Func<TransitionEvent, Task> PageBackRequested;
        public event Func<object, TransitionRequest, Task> PageTransitionRequested;

        #endregion
    }
}