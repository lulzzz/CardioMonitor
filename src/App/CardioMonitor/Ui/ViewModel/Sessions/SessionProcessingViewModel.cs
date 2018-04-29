using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.SessionProcessing;
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


namespace CardioMonitor.Ui.ViewModel.Sessions
{

    public abstract class BaseSessionViewModel : SessionProcessor
    {
    }

    /// <summary>
    /// ViewModel для сеанса
    /// </summary>
    //todo last
    public class SessionProcessingViewModel : BaseSessionViewModel, IStoryboardPageViewModel
    {
        #region Поля

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
        [NotNull]
        private readonly IDeviceControllerFactory _deviceControllerFactory;

        [NotNull] private readonly IWorkerController _workerController;

        [NotNull] private readonly IDeviceConfigurationService _deviceConfigurationService;

        private bool _isResultSaved;

        private bool _isSessionStarted;
        private bool _isSessionCompleted;

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
        public ObservableCollection<Patient> Patients => (null != Patient) 
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
        
        
        /// <summary>
        /// Помощник потоков для выполнения фунциий в потоке GUI
        /// </summary>
        public ThreadAssistant ThreadAssistant { get; set; }

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

        /// <summary>
        /// ViewModel для сеанса
        /// </summary>
        public SessionProcessingViewModel(
            ILogger logger,
            IFilesManager filesRepository,
            ISessionsService sessionsService,
            [NotNull] IDeviceControllerFactory deviceControllerFactory,
            [NotNull] IWorkerController workerController, 
            [NotNull] IDeviceConfigurationService deviceConfigurationService) 
        {

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _filesRepository = filesRepository ?? throw new ArgumentNullException(nameof(filesRepository));
            _sessionsService = sessionsService ?? throw new ArgumentNullException(nameof(sessionsService));
            _deviceControllerFactory = deviceControllerFactory ?? throw new ArgumentNullException(nameof(deviceControllerFactory));
            _workerController = workerController;
            _deviceConfigurationService = deviceConfigurationService ?? throw new ArgumentNullException(nameof(deviceConfigurationService));

            //todo for what?
           // Session = new SessionModel();
        }

        /// <summary>
        /// Обрабатывает нажатие на кнопку старт/пауза
        /// </summary>
        private async Task StartButtonClickAsync()
        {
            var actionName = String.Empty;
            try
            {
                switch (SessionStatus)
                {
                    case SessionStatus.InProgress:
                        actionName = "паузы";
                        await PauseAsync().ConfigureAwait(true);
                        break;
                    case SessionStatus.Suspended:
                        actionName = "продолжения";
                        await ResumeAsync().ConfigureAwait(true);
                        break;
                    default:
                        actionName = "старта";
                        await InitAsync().ConfigureAwait(true);
                        await StartAsync().ConfigureAwait(true);
                        break;
                }
            }
            catch (Exception e)
            {
                await MessageHelper.Instance.ShowMessageAsync($"Ошибка {actionName} сеанса").ConfigureAwait(true);
                _logger.Error($"{GetType().Name}: Ошибка {actionName} сеанса. Причина: {e.Message}", e);
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
                    context.MaxAngleX,
                    context.CyclesCount,
                    context.MovementFrequency,
                    bedSavedConfig.ParamsJson);

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
                    _logger);

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
        private Task ReverseButtonClickAsync()
        {
            return ReverseAsync();

        }

        /// <summary>
        /// Прерывает сеанс
        /// </summary>
        private Task EmergencyStopButtonClickAsync()
        {
            return EmeregencyStopAsync();
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

        #region IStoryboardPageViewModel

        public Guid PageId { get; set; }
        public Guid StoryboardId { get; set; }

        public Task OpenAsync(IStoryboardPageContext context)
        {
            if (!(context is SessionProcessingPageConext temp)) throw new ArgumentException($"Context must be {typeof(SessionProcessingPageConext)}");
            _context = temp;
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
