using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.SessionProcessing;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Configuration;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.EventHandlers.Devices;
using CardioMonitor.EventHandlers.Patients;
using CardioMonitor.Ui.Base;
using JetBrains.Annotations;
using Markeli.Storyboards;
using Markeli.Utils.Logging;
using ToastNotifications.Messages;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    public class SessionProcessingInitViewModel : Notifier, IStoryboardPageViewModel, IDataErrorInfo
    {
        private readonly float Tolerance = 1e-4f;
        
        #region Fields

        [NotNull]
        private readonly IDeviceConfigurationService _configurationService;
        private readonly ISessionParamsValidator _sessionParamsValidator;
        private readonly IPatientsService _patientsService;
        private readonly ILogger _logger;
        private ICommand _startCommand;

        private int? _previouslySelectedPatientIdFromContext;

        private IReadOnlyList<PatientFullName> _patients;
        private PatientFullName _selectedPatient;

        private float _maxAngleX;
        private short _cyclesCount;
        private double _movementFrequency;
        private bool _isAutoPumpingEnabled;
        private short _pumpingNumberOfAttemptsOnStartAndFinish;
        private short _pumpingNumberOfAttemptsOnProcessing;
        private bool _isBusy;
        private string _busyMessage;
        
        private ObservableCollection<DeviceConfigInfo> _monitorConfigs;
        private DeviceConfigInfo _selectedMonitorConfig;
        
        private ObservableCollection<DeviceConfigInfo> _bedControllerConfigs;
        private DeviceConfigInfo _selectedBedControllerConfig;

        [NotNull]
        private readonly ToastNotifications.Notifier _notifier;

        [NotNull]
        private readonly PatientAddedEventHandler _patientAddedEventHandler;
        [NotNull]
        private readonly PatientChangedEventHandler _patientChangedEventHandler;
        [NotNull]
        private readonly PatientDeletedEventHandler _patientDeletedEventHandler;

        private bool _isPatientsListChanged;

        [NotNull]
        private readonly DeviceConfigAddedEventHandler _deviceConfigAddedEventHandler;
        [NotNull]
        private readonly DeviceConfigChangedEventHandler _deviceConfigChangedEventHandler;
        [NotNull]
        private readonly DeviceConfigDeletedEventHandler _deviceConfigDeletedEventHandler;

        private bool _isDeviceConfigsListChanged;
        #endregion

        #region Properties


        public IReadOnlyList<PatientFullName> Patients
        {
            get => _patients;
            set
            {
                if (Equals(_patients, value)) return;

                _patients = value;
                RisePropertyChanged(nameof(Patients));
            }
        }


        public PatientFullName SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (Equals(_selectedPatient, value)) return;

                _selectedPatient = value;
                RisePropertyChanged(nameof(SelectedPatient));
                RisePropertyChanged(nameof(StartSessionCommand));
            }
        }


        /// <summary>
        /// Максимальный угол кровати по оси Х, до которой она будет подниматься
        /// </summary>
        public float MaxAngleX
        {
            get => _maxAngleX;
            set
            {
                if (Equals(_maxAngleX, value)) return;
                _maxAngleX = value;
                RisePropertyChanged(nameof(MaxAngleX));
                RisePropertyChanged(nameof(StartSessionCommand));
            }

        }

        /// <summary>
        /// Количество циклов (повторений)
        /// </summary>
        public short CyclesCount
        {
            get => _cyclesCount;
            set
            {
                if (Equals(_cyclesCount, value)) return;
                _cyclesCount = value;
                RisePropertyChanged(nameof(CyclesCount));
                RisePropertyChanged(nameof(StartSessionCommand));
            }

        }

        /// <summary>
        /// Частота движения
        /// </summary>
        public double MovementFrequency
        {
            get => _movementFrequency;
            set
            {
                if (Math.Abs(_movementFrequency - value) < Tolerance) return;
                _movementFrequency = (double)Math.Round((Decimal)value, 3, MidpointRounding.AwayFromZero);
                RisePropertyChanged(nameof(MovementFrequency));
                RisePropertyChanged(nameof(StartSessionCommand));
            }

        }

        /// <summary>
        /// Частота движения
        /// </summary>
        public bool IsAutopumpingEnabled
        {
            get => _isAutoPumpingEnabled;
            set
            {
                if (Equals(_isAutoPumpingEnabled, value)) return;
                _isAutoPumpingEnabled = value;
                RisePropertyChanged(nameof(IsAutopumpingEnabled));
                RisePropertyChanged(nameof(StartSessionCommand));
            }

        }

        /// <summary>
        /// Количество попыток накачки при старте и финише
        /// </summary>
        public short PumpingNumberOfAttemptsOnStartAndFinish
        {
            get => _pumpingNumberOfAttemptsOnStartAndFinish;
            set
            {
                if (value == _pumpingNumberOfAttemptsOnStartAndFinish) return;
                _pumpingNumberOfAttemptsOnStartAndFinish = value;
                RisePropertyChanged(nameof(PumpingNumberOfAttemptsOnStartAndFinish));
                RisePropertyChanged(nameof(StartSessionCommand));
            }
        }

        /// <summary>
        /// Количество попыток накачики в процессе выполнения сеанса
        /// </summary>
        public short PumpingNumberOfAttemptsOnProcessing
        {
            get => _pumpingNumberOfAttemptsOnProcessing;
            set
            {
                if (value == _pumpingNumberOfAttemptsOnProcessing) return;
                _pumpingNumberOfAttemptsOnProcessing = value;
                RisePropertyChanged(nameof(PumpingNumberOfAttemptsOnProcessing));
                RisePropertyChanged(nameof(StartSessionCommand));
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

        public ObservableCollection<DeviceConfigInfo> MonitorConfigs
        {
            get => _monitorConfigs;
            set
            {
                _monitorConfigs = value; 
                RisePropertyChanged(nameof(MonitorConfigs));
            }
        }

        public DeviceConfigInfo SelectedMonitorConfig
        {
            get => _selectedMonitorConfig;
            set
            {
                _selectedMonitorConfig = value;
                RisePropertyChanged(nameof(SelectedMonitorConfig));
            }
        }

        public ObservableCollection<DeviceConfigInfo> BedControllerConfigs
        {
            get => _bedControllerConfigs;
            set
            {
                _bedControllerConfigs = value;
                RisePropertyChanged(nameof(BedControllerConfigs));
            }
        }

        public DeviceConfigInfo SelectedBedControllerConfig
        {
            get => _selectedBedControllerConfig;
            set
            {
                _selectedBedControllerConfig = value;
                RisePropertyChanged(nameof(SelectedBedControllerConfig));
            }
        }

        #endregion

        #region Commands

        public ICommand StartSessionCommand
        {
            get
            {
                return _startCommand ?? (_startCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => IsValid,
                    ExecuteDelegate = async x => await StartSessionAsync().ConfigureAwait(true)
                });
            }
        }

        #endregion

        #region DefaultValues


        public short MinCyclesCount => SessionParamsConstants.MinCyclesCount;

        public short MaxCyclesCount => SessionParamsConstants.MaxCyclesCount;

        
        public float MinValueMaxXAngle => SessionParamsConstants.MinValueMaxXAngle;

        public float MaxValueMaxXAngle => SessionParamsConstants.MaxValueMaxXAngle;

        public float MaxXAngleStep => SessionParamsConstants.MaxXAngleStep;


        public double MinMovementFrequency => SessionParamsConstants.MinMovementFrequency;

        public double MaxMovementFrequency => SessionParamsConstants.MaxMovementFrequency;

        public double MovementFrequencyStep => SessionParamsConstants.MovementFrequencyStep;


        public short MinPumpingNumberOfAttemptsOnStartAndFinish => SessionParamsConstants.MinPumpingNumberOfAttemptsOnStartAndFinish;

        public short MaxPumpingNumberOfAttemptsOnStartAndFinish => SessionParamsConstants.MaxPumpingNumberOfAttemptsOnStartAndFinish;

        public short MinPumpingNumberOfAttemptsOnProcessing => SessionParamsConstants.MinPumpingNumberOfAttemptsOnProcessing;

        public short MaxPumpingNumberOfAttemptsOnProcessing => SessionParamsConstants.MaxPumpingNumberOfAttemptsOnProcessing;

        #endregion

        public SessionProcessingInitViewModel(
            [NotNull] IPatientsService patientsService,
            [NotNull] ILogger logger,
            [NotNull] ISessionParamsValidator sessionParamsValidator,
            [NotNull] PatientAddedEventHandler patientAddedEventHandler,
            [NotNull] PatientChangedEventHandler patientChangedEventHandler,
            [NotNull] PatientDeletedEventHandler patientDeletedEvent,
            [NotNull] IDeviceConfigurationService configurationService, 
            [NotNull] ToastNotifications.Notifier notifier,
            [NotNull] DeviceConfigAddedEventHandler deviceConfigAddedEventHandler, 
            [NotNull] DeviceConfigChangedEventHandler deviceConfigChangedEventHandler, 
            [NotNull] DeviceConfigDeletedEventHandler deviceConfigDeletedEventHandler)
        {
            _patientsService = patientsService ?? throw new ArgumentNullException(nameof(patientsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sessionParamsValidator = sessionParamsValidator ?? throw new ArgumentNullException(nameof(sessionParamsValidator));
            _patientAddedEventHandler = patientAddedEventHandler ?? throw new ArgumentNullException(nameof(patientAddedEventHandler));
            _patientChangedEventHandler = patientChangedEventHandler ?? throw new ArgumentNullException(nameof(patientChangedEventHandler));
            _patientDeletedEventHandler = patientDeletedEvent ?? throw new ArgumentNullException(nameof(patientDeletedEvent));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
            _deviceConfigAddedEventHandler = deviceConfigAddedEventHandler ?? throw new ArgumentNullException(nameof(deviceConfigAddedEventHandler));
            _deviceConfigChangedEventHandler = deviceConfigChangedEventHandler ?? throw new ArgumentNullException(nameof(deviceConfigChangedEventHandler));
            _deviceConfigDeletedEventHandler = deviceConfigDeletedEventHandler ?? throw new ArgumentNullException(nameof(deviceConfigDeletedEventHandler));

            _patientAddedEventHandler.PatientAdded += (sender, args) => _isPatientsListChanged = true;
            _patientChangedEventHandler.PatientChanged += (sender, args) => _isPatientsListChanged = true;
            _patientDeletedEventHandler.PatientDeleted += (sender, args) => _isPatientsListChanged = true;

            _isPatientsListChanged = false;

            _deviceConfigAddedEventHandler.DeviceConfigAdded += (sender, args) => _isDeviceConfigsListChanged = true;
            _deviceConfigChangedEventHandler.DeviceConfigChanged += (sender, args) => _isDeviceConfigsListChanged = true;
            _deviceConfigDeletedEventHandler.DeviceConfigDeleted += (sender, args) => _isDeviceConfigsListChanged = true;

            _isDeviceConfigsListChanged = false;
        }

        public void Dispose()
        {
            _patientAddedEventHandler.Unsubscribe();
            _patientChangedEventHandler.Unsubscribe();
            _patientDeletedEventHandler.Unsubscribe();

            _patientAddedEventHandler.Dispose();
            _patientChangedEventHandler.Dispose();
            _patientDeletedEventHandler.Dispose();

            _deviceConfigAddedEventHandler.Unsubscribe();
            _deviceConfigChangedEventHandler.Unsubscribe();
            _deviceConfigDeletedEventHandler.Unsubscribe();
            
            _deviceConfigAddedEventHandler.Dispose();
            _deviceConfigChangedEventHandler.Dispose();
            _deviceConfigDeletedEventHandler.Dispose();
        }

        private async Task StartSessionAsync()
        {
            await PageTransitionRequested.InvokeAsync(this,
                new TransitionRequest(PageIds.SessionProcessingPageId, new SessionProcessingPageConext
                {
                    PatientId = SelectedPatient.PatientId,
                    MaxAngleX = MaxAngleX,
                    CyclesCount = CyclesCount,
                    IsAutopumpingEnabled = IsAutopumpingEnabled,
                    MovementFrequency = (float)MovementFrequency,
                    PumpingNumberOfAttemptsOnProcessing = PumpingNumberOfAttemptsOnProcessing,
                    PumpingNumberOfAttemptsOnStartAndFinish = PumpingNumberOfAttemptsOnStartAndFinish,
                    InverstionTableConfigId = SelectedBedControllerConfig.Id,
                    MonitorConfigId = SelectedMonitorConfig.Id
                })).ConfigureAwait(true);
        }

        private async Task InitPageAsync(IStoryboardPageContext context, bool updateDevices, bool updatePatients)
        {
            if (!(context is SessionProcessingInitPageContext pageContext))
                throw new ArgumentException("Incorrect argument");
            SetDefaultValues();
            if (_previouslySelectedPatientIdFromContext == pageContext.PatientId
                && Patients != null
                && !_isPatientsListChanged) return;
            var temp = this[String.Empty];
            if (updateDevices)
            {
                await LoadDevicesAsync().ConfigureAwait(false);
            }

            if (updatePatients)
            {
                await LoadPatientsAsync(pageContext.PatientId).ConfigureAwait(false);
            }
        }

        private void SetDefaultValues()
        {
            SelectedBedControllerConfig = null;
            SelectedMonitorConfig = null;
            SelectedPatient = null;
            MaxAngleX = SessionParamsConstants.DefaultMaxXAngle;
            CyclesCount = SessionParamsConstants.DefaultCyclesCount;
            MovementFrequency = SessionParamsConstants.DefaultMovementFrequency;
            PumpingNumberOfAttemptsOnProcessing = SessionParamsConstants.DefaultPumpingNumberOfAttemptsOnProcessing;
            PumpingNumberOfAttemptsOnStartAndFinish =
                SessionParamsConstants.DefaultPumpingNumberOfAttemptsOnStartAndFinish;
            IsAutopumpingEnabled = true;
        }

        private async Task LoadDevicesAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Загрузка конфигураций устройств...";
                var monitorConfigs = await _configurationService
                    .GetConfigurationsAsync(MonitorDeviceTypeId.DeviceTypeId)
                    .ConfigureAwait(false);

                MonitorConfigs = new ObservableCollection<DeviceConfigInfo>(monitorConfigs.Select(x =>
                    new DeviceConfigInfo
                    {
                        Id = x.ConfigId,
                        Name = x.ConfigName
                    }));
                if (SelectedMonitorConfig != null)
                {
                    SelectedMonitorConfig =
                        MonitorConfigs.FirstOrDefault(x => x.Id == SelectedMonitorConfig.Id);

                }

                var bedControllersConfigs = await _configurationService
                    .GetConfigurationsAsync(InversionTableDeviceTypeId.DeviceTypeId)
                    .ConfigureAwait(false);
                BedControllerConfigs = new ObservableCollection<DeviceConfigInfo>(bedControllersConfigs.Select(x =>
                    new DeviceConfigInfo
                    {
                        Id = x.ConfigId,
                        Name = x.ConfigName
                    }));

                if (SelectedBedControllerConfig != null)
                {
                    SelectedBedControllerConfig =
                        BedControllerConfigs.FirstOrDefault(x => x.Id == SelectedBedControllerConfig.Id);
                }

                _isDeviceConfigsListChanged = false;
            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: ошибка получение конфигурации устройств. Причина: {e.Message}", e);
                _notifier.ShowError("Ошибка загрузки конфигураций устройств...");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadPatientsAsync(int? selectedPatientId = null)
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Загрузка информации о пациентах...";
                var temp = await _patientsService.GetPatientNamesAsync().ConfigureAwait(true);
                Patients = new List<PatientFullName>(temp);
                if (selectedPatientId.HasValue)
                {
                    SelectedPatient = Patients.FirstOrDefault(x => x.PatientId == selectedPatientId.Value);
                }

                _previouslySelectedPatientIdFromContext = selectedPatientId;
                _isPatientsListChanged = false;
            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: Ошибка обновления списка пациентов. Причина: {e.Message}", e);
                _notifier.ShowError("Ошибка обновления списка пациентов");
            }
            finally
            {
                IsBusy = false;
            }
        }

        #region IStoryboardPageViewModel
        
        public Guid PageId { get; set; }
        public Guid StoryboardId { get; set; }

        public Task OpenAsync(IStoryboardPageContext context)
        {
            _patientAddedEventHandler.Subscribe();
            _patientChangedEventHandler.Subscribe();
            _patientDeletedEventHandler.Subscribe();


            _deviceConfigAddedEventHandler.Subscribe();
            _deviceConfigChangedEventHandler.Subscribe();
            _deviceConfigDeletedEventHandler.Subscribe();

            Task.Factory.StartNew(async () => await InitPageAsync(context, true, true).ConfigureAwait(false)).ConfigureAwait(false);
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
            if (_isDeviceConfigsListChanged || _isPatientsListChanged)
            {
                Task.Factory.StartNew(async () => await InitPageAsync(
                        context,
                        _isDeviceConfigsListChanged,
                        _isPatientsListChanged)
                    .ConfigureAwait(false));
            }
            return Task.CompletedTask;
        }

        public Task<bool> CanCloseAsync()
        {
            return Task.FromResult(true);
        }

        public Task CloseAsync()
        {

            _patientAddedEventHandler.Unsubscribe();
            _patientChangedEventHandler.Unsubscribe();
            _patientDeletedEventHandler.Unsubscribe();

            _deviceConfigAddedEventHandler.Unsubscribe();
            _deviceConfigChangedEventHandler.Unsubscribe();
            _deviceConfigDeletedEventHandler.Unsubscribe();

            return Task.CompletedTask;
        }

        public event Func<TransitionEvent, Task> PageCanceled;
        public event Func<TransitionEvent, Task> PageCompleted;
        public event Func<TransitionEvent, Task> PageBackRequested;
        public event Func<object, TransitionRequest, Task> PageTransitionRequested;

        #endregion

        #region Validation

        public string this[string columnName]
        {
            get
            {
                if ((String.IsNullOrEmpty(columnName) || columnName == nameof(SelectedMonitorConfig)) && SelectedMonitorConfig == null)
                {
                    return "Нужно выбрать конфигурация контроллера кардиомонитора";
                }
                if ((String.IsNullOrEmpty(columnName) || columnName == nameof(SelectedBedControllerConfig)) && SelectedBedControllerConfig == null)
                {
                    return "Нужно выбрать конфигурация контроллера инверсионного стола";
                }

                if ((String.IsNullOrEmpty(columnName) || columnName == nameof(SelectedPatient)) && SelectedPatient == null)
                {
                    return "Нужно выбрать пациента";
                }

                if (String.IsNullOrEmpty(columnName) || columnName == nameof(MaxAngleX))
                {
                    var result = _sessionParamsValidator.IsMaxXAngleValid(MaxAngleX);
                    if (!result)
                    {
                        return $"Нужно указать максимальный угол по оси X в диапазоне [{SessionParamsConstants.MinValueMaxXAngle}; " +
                               $"{SessionParamsConstants.MaxValueMaxXAngle}]";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || columnName == nameof(CyclesCount))
                {
                    var result = _sessionParamsValidator.IsCyclesCountValid(CyclesCount);
                    if (!result)
                    {
                        return $"Нужно указать количество повторений в диапазоне [{SessionParamsConstants.MinCyclesCount}; " +
                               $"{SessionParamsConstants.MaxCyclesCount}]";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || columnName == nameof(MovementFrequency))
                {
                    var result = _sessionParamsValidator.IsMovementFrequencyValid((float)MovementFrequency);
                    if (!result)
                    {
                        return $"Нужно указать частоту в диапазоне [{SessionParamsConstants.MinMovementFrequency}; " +
                               $"{SessionParamsConstants.MaxMovementFrequency}]";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || IsAutopumpingEnabled && columnName == nameof(PumpingNumberOfAttemptsOnStartAndFinish))
                {
                    var result = _sessionParamsValidator.IsPumpingNumberOfAttemptsOnStartAndFinishValid(PumpingNumberOfAttemptsOnStartAndFinish);
                    if (!result)
                    {
                        return $"Нужно указать число в диапазоне [{SessionParamsConstants.MinPumpingNumberOfAttemptsOnStartAndFinish}; " +
                               $"{SessionParamsConstants.MaxPumpingNumberOfAttemptsOnStartAndFinish}]";
                    }
                }
                if (String.IsNullOrEmpty(columnName) || IsAutopumpingEnabled && columnName == nameof(PumpingNumberOfAttemptsOnProcessing))
                {
                    var result = _sessionParamsValidator.IsPumpingNumberOfAttemptsOnProcessing(PumpingNumberOfAttemptsOnProcessing);
                    if (!result)
                    {
                        return $"Нужно указать число в диапазоне [{SessionParamsConstants.MinPumpingNumberOfAttemptsOnProcessing}; " +
                               $"{SessionParamsConstants.MaxPumpingNumberOfAttemptsOnProcessing}]";
                    }
                }
                return null;
            }
        }

        public string Error => String.Empty;

        public bool IsValid => String.IsNullOrEmpty(this[String.Empty]);

        #endregion

        public class DeviceConfigInfo
        {
            public Guid Id { get; set; }

            public string Name { get; set; }
        }
    }
}