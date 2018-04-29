using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.SessionProcessing;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Configuration;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Files;
using CardioMonitor.Infrastructure.Threading;
using CardioMonitor.Infrastructure.Workers;
using CardioMonitor.Threading;
using CardioMonitor.Ui.Base;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Markeli.Storyboards;
using Markeli.Utils.Logging;
using IUiInvoker = CardioMonitor.Infrastructure.IUiInvoker;


namespace CardioMonitor.Ui.ViewModel.Sessions
{


    /// <summary>
    /// ViewModel для сеанса
    /// </summary>
    public class SessionProcessingViewModel : SessionProcessor, IStoryboardPageViewModel
    {
        #region Constants

        private readonly string _startText = "Старт";
        private readonly string _pauseText = "Пауза";
        private readonly string _resumeText = "Продолжить";

        #endregion

        #region Fields

        private SessionProcessingPageConext _context;

        private Patient _patient;
        private SessionModel _session;

        private short _repeatCount;

        private string _startButtonText;
        private int _periodSeconds;
        private int _periodNumber;

        private ICommand _startCommand;
        private ICommand _reverseCommand;
        private ICommand _emergencyStopCommand;


        private readonly ILogger _logger;
        private readonly IFilesManager _filesRepository;
        private readonly ISessionsService _sessionsService;
        [NotNull] private readonly IDeviceControllerFactory _deviceControllerFactory;

        [NotNull] private readonly IWorkerController _workerController;

        [NotNull] private readonly IDeviceConfigurationService _deviceConfigurationService;
        [NotNull] private readonly IPatientsService _patientsService;
        [NotNull] private readonly IUiInvoker _uiInvoker;

        private bool _isResultSaved;

        private bool _isSessionStarted;
        private bool _isSessionCompleted;

        private int _selectedCycleTab;

        private bool _isBusy;
        private string _busyMessage;

        #endregion

        #region Свойства

        /// <summary>
        /// Пациент
        /// </summary>
        public Patient Patient
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

//        /// <summary>
//        /// Сеанс
//        /// </summary>
//        public SessionModel Session
//        {
//            get { return _session; }
//            private set
//            {
//                if (!value.Equals(_session))
//                {
//                    _session = value;
//                    RisePropertyChanged(nameof(Session));
//                   // RisePropertyChanged(nameof(Status));
//                    ElapsedTime = new TimeSpan();
//                    RemainingTime = new TimeSpan();
//                    StartButtonText = Localisation.SessionViewModel_StartButton_Text;
//                }
//            }
//        }




        /// <summary>
        /// Текст кнопки старт
        /// </summary>
        public string StartButtonText
        {
            get => _startButtonText;
            set
            {
                if (value != _startButtonText)
                {
                    _startButtonText = value;
                    RisePropertyChanged(nameof(StartButtonText));
                }
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

        
        private string _executionStatus;

        public string ExecutionStatus
        {
            get { return _executionStatus; }
            set
            {
                if (value != _executionStatus)
                {
                    _executionStatus = value;
                    RisePropertyChanged("ExecutionStatus");
                }
            }
        }

        #endregion

        #region Command

        /// <summary>
        /// Команда старта/пауза сеанса
        /// </summary>
        public ICommand StartCommand
        {
            get
            {
                return _startCommand ?? (_startCommand = new SimpleCommand
                {
                    CanExecuteDelegate = o => true,
                    ExecuteDelegate = async o => await StartButtonClickAsync().ConfigureAwait(true)
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
                    CanExecuteDelegate = o => true,
                    ExecuteDelegate = async o => await ReverseButtonClickAsync().ConfigureAwait(true)
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
                    CanExecuteDelegate = o => true,
                    ExecuteDelegate = async o => await EmergencyStopButtonClickAsync().ConfigureAwait(true)
                });
            }
        }

        #endregion
        /// <summary>
        /// ViewModel для сеанса
        /// </summary>
        public SessionProcessingViewModel(
            [NotNull]ILogger logger,
            [NotNull] IFilesManager filesRepository,
            [NotNull] ISessionsService sessionsService,
            [NotNull] IDeviceControllerFactory deviceControllerFactory,
            [NotNull] IWorkerController workerController, 
            [NotNull] IDeviceConfigurationService deviceConfigurationService,
            [NotNull] IPatientsService patientsService,
                [NotNull] IUiInvoker uiInvoker) 
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _filesRepository = filesRepository ?? throw new ArgumentNullException(nameof(filesRepository));
            _sessionsService = sessionsService ?? throw new ArgumentNullException(nameof(sessionsService));
            _deviceControllerFactory = deviceControllerFactory ?? throw new ArgumentNullException(nameof(deviceControllerFactory));
            _workerController = workerController;
            _deviceConfigurationService = deviceConfigurationService ?? throw new ArgumentNullException(nameof(deviceConfigurationService));
            _patientsService = patientsService ?? throw new ArgumentNullException(nameof(patientsService));
            _uiInvoker = uiInvoker ?? throw new ArgumentNullException(nameof(uiInvoker));

            StartButtonText = _startText;
            //todo for what?
            // Session = new SessionModel();
            OnException += HandleOnException;
        }

        private void HandleOnException(object sender, SessionProcessingException sessionProcessingException)
        {
        }

        /// <summary>
        /// Обрабатывает нажатие на кнопку старт/пауза
        /// </summary>
        private async Task StartButtonClickAsync()
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
                        await ResumeAsync().ConfigureAwait(true);
                        StartButtonText = _pauseText;
                        break;
                    default:
                        actionName = "старта";
                        await InitAsync().ConfigureAwait(true);
                        await StartAsync().ConfigureAwait(true);
                        StartButtonText = _pauseText;
                        break;
                }
            }
            catch (Exception e)
            {
                await MessageHelper.Instance.ShowMessageAsync($"Ошибка {actionName} сеанса").ConfigureAwait(true);
                _logger.Error($"{GetType().Name}: Ошибка {actionName} сеанса. Причина: {e.Message}", e);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task InitAsync()
        {
            _isResultSaved = false;

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
                TimeSpan.FromMilliseconds(300),
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
        /// Завершает сенас
        /// </summary>
        private void SessionComplete()
        {
            //Session.Status = SessionStatus.Completed;
            //todo save later
            // SaveSession();
        }

        /// <summary>
        /// Сохраняет результаты сеанса в базу и файл
        /// </summary>
        private async void SaveSession()
        {
            _isResultSaved = true;
//            var exceptionMessage = String.Empty;
//            try
//            {
//                _filesRepository.SaveToFile(Patient, Session.Session);
//                //todo это работать не будет
//                _sessionsService.AddAsync(Session.Session);
//                await MessageHelper.Instance.ShowMessageAsync(Localisation.SessionViewModel_SessionCompeted);
//            }
//            catch (ArgumentNullException ex)
//            {
//                _logger.LogError("SessionViewModel", ex);
//                exceptionMessage = Localisation.SessionViewModel_SaveSession_ArgumentNullException;
//            }
//            catch (Exception ex)
//            {
//                exceptionMessage = ex.Message;
//            }
//            if (!String.IsNullOrEmpty(exceptionMessage))
//            {
//                await MessageHelper.Instance.ShowMessageAsync(exceptionMessage);
//            }
        }

        /// <summary>
        /// Реверс
        /// </summary>
        private async Task ReverseButtonClickAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Реверс сеанса...";
                await ReverseAsync().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: Ошибка реверса. Причина: {e.Message}", e);
                await MessageHelper.Instance.ShowMessageAsync("Ошибка реверса");
            }
            finally
            {
                IsBusy = false;
            }


        }

        /// <summary>
        /// Прерывает сеанс
        /// </summary>
        private async Task EmergencyStopButtonClickAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Экстренная остановка...";
                await EmeregencyStopAsync().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                _logger.Error($"{GetType().Name}: Ошибка экстренной остановки. Причина: {e.Message}", e);
                await MessageHelper.Instance.ShowMessageAsync("Ошибка экстренной остановки");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Очищает содержимое ViewModel, обнуляя все данные
        /// </summary>
        public void Clear()
        {
            Patient = null;
            RemainingTime = TimeSpan.Zero;
            ElapsedTime = TimeSpan.Zero;
            //Session = new SessionModel();
        }

        private async Task InternalOpenAsync(SessionProcessingPageConext context)
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Подготовка сеанса...";
                var patient = await _patientsService.GetPatientAsync(context.PatientId).ConfigureAwait(false);

                _uiInvoker.Invoke(() => { Patient = patient;});
            }
            catch (Exception e)
            {
                await MessageHelper.Instance.ShowMessageAsync("Ошибка подготовки сеанса").ConfigureAwait(false);
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
                var result = await MessageHelper.Instance.ShowMessageAsync("Все данные будут потеряны. Вы уверены?", "Cardio Monitor",
                    MessageDialogStyle.AffirmativeAndNegative);
                return result == MessageDialogResult.Affirmative;
            }

            if (SessionStatus == SessionStatus.EmergencyStopped
                || SessionStatus == SessionStatus.Completed
                || SessionStatus == SessionStatus.TerminatedOnError && !_isResultSaved)
            {

                var result = await MessageHelper.Instance.ShowMessageAsync("Все данные будут потеряны. Вы уверены?", "Cardio Monitor",
                    MessageDialogStyle.AffirmativeAndNegative);
                return result == MessageDialogResult.Affirmative;
            }

            //todo is saved?
            return true;
        }

        public async Task CloseAsync()
        {
            if (_isSessionCompleted) return;
            if (!_isSessionStarted) return;

            await EmeregencyStopAsync();
        }

        public event Func<TransitionEvent, Task> PageCanceled;
        public event Func<TransitionEvent, Task> PageCompleted;
        public event Func<TransitionEvent, Task> PageBackRequested;
        public event Func<object, TransitionRequest, Task> PageTransitionRequested;
    }

    #endregion
}
