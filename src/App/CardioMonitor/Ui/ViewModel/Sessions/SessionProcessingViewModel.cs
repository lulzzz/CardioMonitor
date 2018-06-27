using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.CoreContracts.Session.Events;
using CardioMonitor.BLL.SessionProcessing;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Configuration;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.FileSaving;
using CardioMonitor.Infrastructure.Workers;
using CardioMonitor.Infrastructure.WpfCommon.Base;
using CardioMonitor.Resources;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Markeli.Storyboards;
using Markeli.Utils.EventBus.Contracts;
using Markeli.Utils.Logging;
using ToastNotifications.Messages;
using IUiInvoker = CardioMonitor.Infrastructure.IUiInvoker;
using Notifier = ToastNotifications.Notifier;


namespace CardioMonitor.Ui.ViewModel.Sessions
{


    /// <summary>
    /// ViewModel для сеанса
    /// </summary>
    public class SessionProcessingViewModel : SessionProcessor, IStoryboardPageViewModel
    {
        #region Constants

        private readonly string _startText = "СТАРТ";
        private readonly string _pauseText = "ПАУЗА";
        private readonly string _resumeText = "ПРОДОЛЖИТЬ";

        #endregion

        #region Fields

        private SessionProcessingPageConext _context;

        private Patient _patient;
        private bool _canManualDataCommandExecute;

        private string _startButtonText;

        private ICommand _startCommand;
        private ICommand _reverseCommand;
        private ICommand _emergencyStopCommand;
        private ICommand _manualRequestCommand;
        private ICommand _saveSessionToDbCommand;
        private ICommand _saveSessionToFileCommand;

        private readonly ILogger _logger;
        private readonly ISessionFileSavingManager _sessionFileSavingRepository;
        private readonly ISessionsService _sessionsService;
        [NotNull] private readonly IDeviceControllerFactory _deviceControllerFactory;

        [NotNull] private readonly IWorkerController _workerController;

        [NotNull] private readonly IDeviceConfigurationService _deviceConfigurationService;
        [NotNull] private readonly IPatientsService _patientsService;
        [NotNull] private readonly IUiInvoker _uiInvoker;

        [NotNull] private readonly Notifier _notifier;

        [NotNull] private readonly IEventBus _eventBus;

        [CanBeNull] private int? _savedSessionId;

        private bool _isSessionSaved;

        private DateTime _sessionTimestampUtc;

        private bool _isSessionStarted;

        private int _selectedCycleTab;

        private bool _isBusy;
        private string _busyMessage;

        private bool _isReverseAlreadyRequested;

        private bool _isAutoPumpingChangingEnabled;

        private bool _isAutoPumpingEnabled;

        #endregion

        #region Свойства


        /// <summary>
        /// Пациент
        /// </summary>
        private Patient Patient
        {
            get => _patient;
            set
            {
                if (Equals(value, _patient)) return;
                _patient = value;
                RisePropertyChanged(nameof(Patient));
                RisePropertyChanged(nameof(Patients));
            }
        }

        /// <summary>
        /// Список пациентов
        /// </summary>
        /// <remarks>
        /// Лайфхак для отображения пациента в таблице
        /// </remarks>
        public ObservableCollection<Patient> Patients => null != Patient
            ? new ObservableCollection<Patient> {Patient}
            : new ObservableCollection<Patient>();


        /// <summary>
        /// Текст кнопки старт
        /// </summary>
        public string StartButtonText
        {
            get => _startButtonText;
            private set
            {
                if (value == _startButtonText) return;
                _startButtonText = value;
                RisePropertyChanged(nameof(StartButtonText));
            }
        }

        public int SelectedCycleTab
        {
            get => _selectedCycleTab;
            set
            {
                _selectedCycleTab = value;
                RisePropertyChanged(nameof(SelectedCycleTab));
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                _isBusy = value;
                RisePropertyChanged(nameof(IsBusy));
            }
        }

        public string BusyMessage
        {
            get => _busyMessage;
            private set
            {
                _busyMessage = value;
                RisePropertyChanged(nameof(BusyMessage));
            }
        }

        public bool IsSessionStarted
        {
            get => _isSessionStarted;
            private set
            {
                _isSessionStarted = value;
                RisePropertyChanged(nameof(IsSessionStarted));
            }
        }


        private bool CanManualDataCommandExecute
        {
            get => _canManualDataCommandExecute;
            set
            {
                _canManualDataCommandExecute = value;
                RisePropertyChanged(nameof(CanManualDataCommandExecute));
                RisePropertyChanged(nameof(ManualRequestCommand));
            }
        }

        private bool IsReverseAlreadyRequested
        {
            get => _isReverseAlreadyRequested;
            set
            {
                _isReverseAlreadyRequested = value;
                RisePropertyChanged(nameof(IsReverseAlreadyRequested));
                RisePropertyChanged(nameof(ReverseCommand));
            }
        }


        public bool IsAutoPumpingChangingEnabled
        {
            get => _isAutoPumpingChangingEnabled;
            private set
            {
                _isAutoPumpingChangingEnabled = value;
                RisePropertyChanged(nameof(IsAutoPumpingChangingEnabled));
            }
        }


        public bool IsAutoPumpingEnabled
        {
            get => _isAutoPumpingEnabled;
            set
            {
                var previousValue = _isAutoPumpingEnabled;
                _isAutoPumpingEnabled = value;

                if (SessionStatus == SessionStatus.InProgress)
                {

                    try
                    {
                        UpdateAutoPumpingStateUnsafe(value);
                    }
                    catch (Exception e)
                    {
                        _notifier.ShowError("Не удалось изменить статус автонакачки");
                        _logger.Error($"{GetType().Name}: Ошибка изменения статуса автоначки. Причина: {e.Message}");
                        _isAutoPumpingEnabled = previousValue;
                    }
                }

                RisePropertyChanged(nameof(IsAutoPumpingEnabled));
            }
        }

        private bool IsSessionSaved
        {
            get => _isSessionSaved;
            set
            {
                _isSessionSaved = value;
                RisePropertyChanged(nameof(IsSessionSaved));
                RisePropertyChanged(nameof(IsSavingToDbEnabled));

            }
        }

        private int? SavedSessionId
        {
            get => _savedSessionId;
            set
            {
                _savedSessionId = value;
                RisePropertyChanged(nameof(SavedSessionId));
                IsSessionSaved = true;
            }
        }

        public bool IsSavingToDbEnabled => (SessionStatus == SessionStatus.Completed
                                              || SessionStatus == SessionStatus.EmergencyStopped
                                              || SessionStatus == SessionStatus.TerminatedOnError)
                                             && !IsSessionSaved;

        #endregion

        #region Commands

        /// <summary>
        /// Команда старта/пауза сеанса
        /// </summary>
        public ICommand StartCommand
        {
            get
            {
                return _startCommand ?? (_startCommand = new SimpleCommand
                {
                    CanExecuteDelegate = o => CanStartCommandExecute(),
                    ExecuteDelegate = async o => await StartCommandExecuteAsync().ConfigureAwait(true)
                });
            }
        }

        /// <summary>
        /// Команда реверса
        /// </summary>
        public ICommand ReverseCommand
        {
            get
            {
                return _reverseCommand ?? (_reverseCommand = new SimpleCommand
                {
                    CanExecuteDelegate = o => CanReverseCommandExecute(),
                    ExecuteDelegate = async o => await ReverseCommandExecuteAsync().ConfigureAwait(true)
                });
            }
        }

        /// <summary>
        /// Команда экстренной остановки
        /// </summary>
        public ICommand EmergencyStopCommand
        {
            get
            {
                return _emergencyStopCommand ?? (_emergencyStopCommand = new SimpleCommand
                {
                    CanExecuteDelegate = o => CanEmergencyCommandExecute(),
                    ExecuteDelegate = async o => await EmergencyCommandExecuteAsync().ConfigureAwait(true)
                });
            }
        }

        /// <summary>
        /// Команда ручного обновления данных после завершения сеанса
        /// </summary>
        public ICommand ManualRequestCommand
        {
            get
            {
                return _manualRequestCommand ?? (_manualRequestCommand = new SimpleCommand
                {
                    CanExecuteDelegate = o => CanManualDataCommandExecute,
                    ExecuteDelegate = async o => await ManualDataCommandExecuteAsync().ConfigureAwait(true)
                });
            }
        }

        public ICommand SaveSessionToDbCommand
        {
            get
            {
                return _saveSessionToDbCommand ?? (_saveSessionToDbCommand = new SimpleCommand
                {
                    CanExecuteDelegate = o => true,
                    ExecuteDelegate = async o => await SaveSessionToDbAsync().ConfigureAwait(true)
                });
            }
        }

        public ICommand SaveSessionToFileCommand
        {
            get
            {
                return _saveSessionToFileCommand ?? (_saveSessionToFileCommand = new SimpleCommand
                {
                    CanExecuteDelegate = o => true,
                    ExecuteDelegate = async o => await SaveSessionToFileAsync().ConfigureAwait(true)
                });
            }
        }

        #endregion

        /// <summary>
        /// ViewModel для сеанса
        /// </summary>
        public SessionProcessingViewModel(
            [NotNull] ILogger logger,
            [NotNull] ISessionFileSavingManager sessionFileSavingRepository,
            [NotNull] ISessionsService sessionsService,
            [NotNull] IDeviceControllerFactory deviceControllerFactory,
            [NotNull] IWorkerController workerController,
            [NotNull] IDeviceConfigurationService deviceConfigurationService,
            [NotNull] IPatientsService patientsService,
            [NotNull] IUiInvoker uiInvoker,
            [NotNull] Notifier notifier,
            [NotNull] IEventBus eventBus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sessionFileSavingRepository = sessionFileSavingRepository ?? throw new ArgumentNullException(nameof(sessionFileSavingRepository));
            _sessionsService = sessionsService ?? throw new ArgumentNullException(nameof(sessionsService));
            _deviceControllerFactory = deviceControllerFactory ??
                                       throw new ArgumentNullException(nameof(deviceControllerFactory));
            _workerController = workerController;
            _deviceConfigurationService = deviceConfigurationService ??
                                          throw new ArgumentNullException(nameof(deviceConfigurationService));
            _patientsService = patientsService ?? throw new ArgumentNullException(nameof(patientsService));
            _uiInvoker = uiInvoker ?? throw new ArgumentNullException(nameof(uiInvoker));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            CanManualDataCommandExecute = true;
            IsAutoPumpingChangingEnabled = true;
            StartButtonText = _startText;
            OnException += HandleOnException;
            OnSessionErrorStop += HandleSessionErrorStop;
            OnSessionCompleted += HandleSessionCompleted;
            OnPausedFromDevice += HanlePausedFromDevice;
            OnResumedFromDevice += HandleResumedFromDevice;
            OnEmeregencyStoppedFromDevice += HandleEmeregencyStoppedFromDevice;
            OnReversedFromDevice += HandleReversedFromDevice;
            OnSessionStatusChanged += HandleSessionStatusChanged;
            OnCycleCompleted += HandleCycleCompleted;
        }



        #region Device events hadnling

        private void HandleCycleCompleted(object sender, short completedCycleNumber)
        {
            if (completedCycleNumber == PatientParamsPerCycles.Count) return;

            _notifier.ShowInformation($"Завершилось повторение № {completedCycleNumber}");

            // если выбрана другая вкладка, то ничего делать не будем
            if (SelectedCycleTab != completedCycleNumber - 1) return;
            SelectedCycleTab = completedCycleNumber;
        }

        private void HandleReversedFromDevice(object sender, EventArgs eventArgs)
        {
            _uiInvoker.Invoke(() => { IsReverseAlreadyRequested = true; });

            _notifier.ShowInformation("С инверсионного стола запущен рерверс");
        }

        private void HandleSessionCompleted(object sender, EventArgs eventArgs)
        {
            Task.Factory.StartNew(async () => await SaveSessionToDbAsync());
            _notifier.ShowSuccess(Localisation.SessionViewModel_SessionCompeted);
        }

        private void HandleResumedFromDevice(object sender, EventArgs eventArgs)
        {
            _notifier.ShowInformation("С инверсионного стола продолжен сеанс");
        }

        private void HanlePausedFromDevice(object sender, EventArgs eventArgs)
        {
            _notifier.ShowInformation("С инверсионного стола сеанс приостановлен");
        }

        private void HandleEmeregencyStoppedFromDevice(object sender, EventArgs eventArgs)
        {
            _notifier.ShowWarning("С инверсионного стола вызвана экстренная остановка сеанса");
            Task.Factory.StartNew(async () => await SaveSessionToDbAsync());
            // не хочет лезть в родительский класс, поэтому такой костыль
            RisePropertyChanged(nameof(IsSavingToDbEnabled));
        }

        private void HandleSessionErrorStop(object sender, Exception exception)
        {
            _notifier.ShowError("Сеанс прерван из-за непредвиденной ошибки");
            Task.Factory.StartNew(async () => await SaveSessionToDbAsync());
        }

        private void HandleSessionStatusChanged(object sender, EventArgs eventArgs)
        {
            RisePropertyChanged(nameof(StartCommand));
            RisePropertyChanged(nameof(ReverseCommand));
            RisePropertyChanged(nameof(EmergencyStopCommand));
            RisePropertyChanged(nameof(ManualRequestCommand));
            RisePropertyChanged(nameof(IsSavingToDbEnabled));

            IsAutoPumpingChangingEnabled = SessionStatus == SessionStatus.InProgress
                                           || SessionStatus == SessionStatus.Suspended
                                           || SessionStatus == SessionStatus.NotStarted;
        }

        private void HandleOnException(object sender, SessionProcessingException sessionProcessingException)
        {
            _notifier.ShowWarning($"Произошла ошибка в ходе выполнения сеанса: {sessionProcessingException.Message}");
        }

        #endregion

        private bool CanStartCommandExecute()
        {
            return SessionStatus == SessionStatus.NotStarted
                   || SessionStatus == SessionStatus.InProgress
                   || SessionStatus == SessionStatus.Suspended;
        }

        private void UpdateAutoPumpingStateUnsafe(bool isAutoPumpingEnabled)
        {
            if (isAutoPumpingEnabled)
            {
                EnableAutoPumping();
            }
            else
            {
                DisableAutoPumping();
            }
        }

        /// <summary>
        /// Обрабатывает нажатие на кнопку старт/пауза
        /// </summary>
        private async Task StartCommandExecuteAsync()
        {
            var actionName = String.Empty;
            try
            {
                IsBusy = true;
                BusyMessage = String.Empty;
                switch (SessionStatus)
                {
                    case SessionStatus.InProgress:
                        actionName = "паузы";
                        await PauseAsync().ConfigureAwait(true);
                        StartButtonText = _resumeText;
                        break;
                    case SessionStatus.Suspended:
                        actionName = "продолжения";
                        UpdateAutoPumpingStateUnsafe(IsAutoPumpingEnabled);
                        await ResumeAsync().ConfigureAwait(true);
                        StartButtonText = _pauseText;
                        break;
                    default:
                        actionName = "старта";
                        await InitAsync().ConfigureAwait(true);
                        UpdateAutoPumpingStateUnsafe(IsAutoPumpingEnabled);
                        await StartAsync().ConfigureAwait(true);
                        StartButtonText = _pauseText;
                        IsSessionStarted = true;
                        _sessionTimestampUtc = DateTime.UtcNow;
                        break;
                }
            }
            catch (Exception e)
            {
                _notifier.ShowError($"Ошибка {actionName} сеанса");
                _logger.Error($"{GetType().Name}: Ошибка {actionName} сеанса. Причина: {e.Message}", e);
            }
            finally
            {
                IsBusy = false;
                BusyMessage = String.Empty;
            }
        }

        private async Task InitAsync()
        {
            _savedSessionId = null;

            var context = _context;

            var bedSavedConfig = await
                _deviceConfigurationService
                    .GetDeviceConfigurationAsync(context.InverstionTableConfigId)
                    .ConfigureAwait(true);

            var bedControllerConfigBuilder = _deviceControllerFactory
                .CreateDeviceControllerConfigBuilder<IBedControllerConfigBuilder>(
                    bedSavedConfig.DeviceId);


            var bedControllerConfig = bedControllerConfigBuilder.Build(
                bedSavedConfig.ParamsJson,
                context.MaxAngleX,
                context.CyclesCount,
                context.MovementFrequency);

            var bedController =
                _deviceControllerFactory
                    .CreateDeviceController<IBedController>(bedSavedConfig.DeviceId);



            var monitorSavedConfig = await
                _deviceConfigurationService
                    .GetDeviceConfigurationAsync(context.MonitorConfigId)
                    .ConfigureAwait(true);

            var monitorControllerConfigBuilder = _deviceControllerFactory
                .CreateDeviceControllerConfigBuilder<IMonitorControllerConfigBuilder>(
                    monitorSavedConfig.DeviceId);


            var monitorInitConfig = monitorControllerConfigBuilder.Build(
                monitorSavedConfig.ParamsJson);

            var monitorController =
                _deviceControllerFactory
                    .CreateDeviceController<IMonitorController>(monitorSavedConfig.DeviceId);


            var startParams = new SessionParams(
                context.CyclesCount,
                //todo в параметры
                TimeSpan.FromMilliseconds(900),
                bedControllerConfig,
                monitorInitConfig,
                context.PumpingNumberOfAttemptsOnStartAndFinish,
                context.PumpingNumberOfAttemptsOnProcessing);

            Init(
                startParams,
                bedController,
                monitorController,
                _workerController,
                _logger,
                _uiInvoker);
            SelectedCycleTab = 0;
        }

        /// <summary>
        /// Сохраняет результаты сеанса в базу и файл
        /// </summary>
        private async Task SaveSessionToDbAsync()
        {
            try
            {
                _uiInvoker.Invoke(() =>
                {
                    IsBusy = true;
                    BusyMessage = "Сохранение результатов сеанса";
                });

                var session = GetSession();
                if (IsSessionSaved)
                {
                    if (SavedSessionId == null) throw new InvalidOperationException("ID сессиия не сохранено");

                    var oldSession = await _sessionsService
                        .GetAsync(SavedSessionId.Value)
                        .ConfigureAwait(true);
                    UpdateIdsInSession(session, oldSession);
                    await _sessionsService
                        .EditAsync(session)
                        .ConfigureAwait(true);
                    await _eventBus
                        .PublishAsync(new SessionChangedEvent(oldSession.Id))
                        .ConfigureAwait(true);
                    _notifier.ShowSuccess("Результаты сеанса обновлены");
                }
                else
                {
                    var sessionId = await _sessionsService
                        .AddAsync(session)
                        .ConfigureAwait(true);
                    SavedSessionId = sessionId;
                    await _eventBus
                        .PublishAsync(new SessionAddedEvent(sessionId))
                        .ConfigureAwait(true);
                    _notifier.ShowSuccess("Результаты сеанса сохранены");
                }
            }
            catch (ArgumentNullException ex)
            {
                _logger.Error($"{GetType().Name}: ошибка сохранения. Причины: {ex.Message}", ex);
                _notifier.ShowError(Localisation.SessionViewModel_SaveSession_ArgumentNullException);
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: ошибка сохранения. Причины: {ex.Message}", ex);
                _notifier.ShowError("Ошибка сохранения сеанса");
            }
            finally
            {
                _uiInvoker.Invoke(() =>
                {
                    IsBusy = false;
                    BusyMessage = null;
                });
            }
        }

        private Session GetSession()
        {
            return new Session
            {
                Status = SessionStatus,
                PatientId = Patient.Id,
                TimestampUtc = _sessionTimestampUtc,
                Cycles = PatientParamsPerCycles
                    .Select(
                        //todo mappers
                        x => new SessionCycle
                        {
                            CycleNumber = x.CycleNumber,
                            PatientParams = x.CycleParams
                                .Select(y => new PatientParams
                                {
                                    AverageArterialPressure = y.AverageArterialPressure,
                                    DiastolicArterialPressure = y.DiastolicArterialPressure,
                                    HeartRate = y.HeartRate,
                                    InclinationAngle = y.InclinationAngle,
                                    Iteraton = y.IterationNumber,
                                    RepsirationRate = y.RespirationRate,
                                    Spo2 = y.Spo2,
                                    SystolicArterialPressure = y.SystolicArterialPressure
                                })
                                .ToList()
                        })
                    .ToList()
            };
        }

        private void UpdateIdsInSession([NotNull] Session newSession, [NotNull] Session sessionFromDb)
        {
            newSession.Id = sessionFromDb.Id;
            if (newSession.Cycles.Count != sessionFromDb.Cycles.Count)
                throw new ArgumentException($"Новая сессия и сессия из базы c ID {sessionFromDb.Id} отличаются количеством повторений");

            for (var i = 0; i < newSession.Cycles.Count; ++i)
            {
                newSession.Cycles[i].Id = sessionFromDb.Cycles[i].Id;
                
                if (newSession.Cycles[i].PatientParams.Count != sessionFromDb.Cycles[i].PatientParams.Count)
                    throw new ArgumentException($"Новая сессия и сессия из базы c ID {sessionFromDb.Id} " +
                                                $"отличаются количеством измерений для повторения с ID {sessionFromDb.Cycles[i].Id}");
                for (var j = 0; j < newSession.Cycles[i].PatientParams.Count; ++j)
                {
                    newSession.Cycles[i].PatientParams[j].Id = sessionFromDb.Cycles[i].PatientParams[j].Id;
                }
            }
        }


        private async Task SaveSessionToFileAsync()
        {
            return;
        }

        private bool CanReverseCommandExecute()
        {
            if (IsReverseAlreadyRequested) return false;

            return SessionStatus == SessionStatus.InProgress;
        }

        /// <summary>
        /// Реверс
        /// </summary>
        private async Task ReverseCommandExecuteAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Реверс сеанса...";
                await ReverseAsync().ConfigureAwait(true);
                IsReverseAlreadyRequested = true;
            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: Ошибка реверса. Причина: {e.Message}", e);
                _notifier.ShowError("Ошибка реверса");
            }
            finally
            {
                IsBusy = false;
                BusyMessage = String.Empty;
            }
            
        }

        private bool CanEmergencyCommandExecute()
        {
            return SessionStatus == SessionStatus.InProgress
                   || SessionStatus == SessionStatus.Suspended;
        }

        /// <summary>
        /// Прерывает сеанс
        /// </summary>
        private async Task EmergencyCommandExecuteAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Экстренная остановка...";
                await EmeregencyStopAsync().ConfigureAwait(true);
                await SaveSessionToDbAsync().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: Ошибка экстренной остановки. Причина: {e.Message}", e);
                _notifier.ShowError("Ошибка экстренной остановки");
            }
            finally
            {
                IsBusy = false;
                BusyMessage = String.Empty;
            }
        }

        private async Task ManualDataCommandExecuteAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Обновление данных для последней точки...";

                await RequestManualDataUpdateAsync().ConfigureAwait(true);
                await SaveSessionToDbAsync().ConfigureAwait(true);

                CanManualDataCommandExecute = false;
            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: Ошибка обновления данных для последней точки. Причина: {e.Message}", e);
                _notifier.ShowError("Ошибка обновления данных для последней точки");
            }
            finally
            {
                IsBusy = false;
                BusyMessage = String.Empty;
            }
        }

        private async Task InternalOpenAsync(SessionProcessingPageConext context)
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Подготовка сеанса...";
                var patient = await _patientsService.GetPatientAsync(context.PatientId).ConfigureAwait(true);

                _uiInvoker.Invoke(() =>
                {
                    IsAutoPumpingEnabled = context.IsAutopumpingEnabled;
                    Patient = patient;
                    IsSessionStarted = false;
                });
            }
            catch (Exception e)
            {
                _notifier.ShowError("Ошибка подготовки сеанса");
                _logger.Error($"{GetType().Name}: Ошибка открытия страницы выполнения сеанса. Причина: {e.Message}", e);
            }
            finally
            {
                IsBusy = false;
                BusyMessage = String.Empty;
            }
        }

        #region IStoryboardPageViewModel

        public Guid PageId { get; set; }
        public Guid StoryboardId { get; set; }

        public Task OpenAsync(IStoryboardPageContext context)
        {
            if (!(context is SessionProcessingPageConext temp)) throw new ArgumentException($"Context must be {typeof(SessionProcessingPageConext)}");
            _context = temp;
            Task.Factory.StartNew(async () => await InternalOpenAsync(_context).ConfigureAwait(false));
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
            if (!(context is SessionProcessingPageConext temp)) throw new ArgumentException($"Context must be {typeof(SessionProcessingPageConext)}");
            _context = temp;

            return Task.CompletedTask;
        }

        public async Task<bool> CanCloseAsync()
        {
            if (SessionStatus == SessionStatus.InProgress
                || SessionStatus == SessionStatus.Suspended)
            {
                var result = await MessageHelper.Instance.ShowMessageAsync(
                    "Сеанс не завершен. Все текущие данные будут потеряны. Вы уверены?", 
                    "Cardio Monitor",
                    MessageDialogStyle.AffirmativeAndNegative);
                return result == MessageDialogResult.Affirmative;
            }

            if ((SessionStatus == SessionStatus.EmergencyStopped
                || SessionStatus == SessionStatus.Completed
                || SessionStatus == SessionStatus.TerminatedOnError) && !IsSessionSaved)
            {

                var result = await MessageHelper.Instance.ShowMessageAsync(
                    "Результаты сеанса не сохранены и будут потеряны. Вы уверены?", 
                    "Cardio Monitor",
                    MessageDialogStyle.AffirmativeAndNegative);
                return result == MessageDialogResult.Affirmative;
            }
            return true;
        }

        public async Task CloseAsync()
        {
            if (SessionStatus == SessionStatus.EmergencyStopped
                || SessionStatus == SessionStatus.Completed
                || SessionStatus == SessionStatus.TerminatedOnError
                || !_isSessionStarted)
            {
                ClearData();
                return;
            }

            try
            {

                await EmeregencyStopAsync().ConfigureAwait(true);
                await SaveSessionToDbAsync().ConfigureAwait(true);
                ClearData();
            }
            catch (Exception e)
            {
                _notifier.ShowError("Ошибка закрытия формы. Экстренная остановка выполнена некорректно или данные не сохранены");
                _logger.Error($"{GetType().Name}: Ошибка закрытия формы. Причина: {e.Message}", e);   
            }
        }
        
        /// <summary>
        /// Очищает содержимое ViewModel, обнуляя все данные
        /// </summary>
        private void ClearData()
        {
            Patient = null;
            RemainingTime = TimeSpan.Zero;
            ElapsedTime = TimeSpan.Zero;
            IsSessionStarted = false;
            SessionStatus = SessionStatus.NotStarted;
            PatientParamsPerCycles = new List<CycleData>(0);
            SavedSessionId = null;
        }

        public event Func<TransitionEvent, Task> PageCanceled;
        public event Func<TransitionEvent, Task> PageCompleted;
        public event Func<TransitionEvent, Task> PageBackRequested;
        public event Func<object, TransitionRequest, Task> PageTransitionRequested;
    }

    #endregion
}
