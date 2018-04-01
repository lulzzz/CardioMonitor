using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.SessionProcessing;
using CardioMonitor.Devices;
using CardioMonitor.Files;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Infrastructure.Threading;
using CardioMonitor.Infrastructure.Workers;
using CardioMonitor.Threading;
using CardioMonitor.Ui.Base;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Markeli.Storyboards;


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

        private bool _isResultSaved;

        #endregion

        #region Свойства

        /// <summary>
        /// Пациент
        /// </summary>
        public Patient Patient
        {
            get { return _patient; }
            set
            {
                if (value != _patient)
                {
                    _patient = value;
                    RisePropertyChanged(nameof(Patient));
                    RisePropertyChanged(nameof(Patients));
                }
            }
        }

        /// <summary>
        /// Список пациентов
        /// </summary>
        /// <remarks>
        /// Лайфхак для отображения пациента в таблице
        /// </remarks>
        public ObservableCollection<Patient> Patients
        {
            get { return (null != Patient) 
                            ? new ObservableCollection<Patient> {Patient} 
                            : new ObservableCollection<Patient>();}
        }

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
        /// Максимальный угол наклона кровати
        /// </summary>
        private float _maxAngle;
        
        
        /// <summary>
        /// Частота
        /// </summary>
        private float _frequency;
        
        
        /// <summary>
        /// Количество попыток накачки при старте и финише
        /// </summary>
        public short PumpingNumberOfAttemptsOnStartAndFinish
        {
            get => _pumpingNumberOfAttemptsOnStartAndFinish;
            set
            {
                if (value != _pumpingNumberOfAttemptsOnStartAndFinish)
                {
                    _pumpingNumberOfAttemptsOnStartAndFinish = value;
                    RisePropertyChanged(nameof(PumpingNumberOfAttemptsOnStartAndFinish));
                }
            }
        }

        private short _pumpingNumberOfAttemptsOnStartAndFinish;
        /// <summary>
        /// Количество попыток накачики в процессе выполнения сеанса
        /// </summary>
        public short PumpingNumberOfAttemptsOnProcessing{
            get => _pumpingNumberOfAttemptsOnStartAndFinish;
            set
            {
                if (value != _pumpingNumberOfAttemptsOnProcessing)
                {
                    _pumpingNumberOfAttemptsOnProcessing = value;
                    RisePropertyChanged(nameof(PumpingNumberOfAttemptsOnProcessing));
                }
            }
        }
        private short _pumpingNumberOfAttemptsOnProcessing;
        
        /// <summary>
        /// Текст кнопки старт
        /// </summary>
        public string StartButtonText
        {
            get { return _startButtonText; }
            set
            {
                if (value != _startButtonText)
                {
                    _startButtonText = value;
                    RisePropertyChanged(nameof(StartButtonText));
                }
            }
        }
      


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
                    ExecuteDelegate = o => StartButtonClickAsync()
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
                    ExecuteDelegate = o => ReverseButtonClickAsync()
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
                    ExecuteDelegate = o => EmergencyStopButtonClickAsync()
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
            TaskHelper taskHelper,
            [NotNull] IDeviceControllerFactory deviceControllerFactory,
            [NotNull] IWorkerController workerController) 
        {
            if (taskHelper == null) throw new ArgumentNullException(nameof(taskHelper));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _filesRepository = filesRepository ?? throw new ArgumentNullException(nameof(filesRepository));
            _sessionsService = sessionsService ?? throw new ArgumentNullException(nameof(sessionsService));
            _deviceControllerFactory = deviceControllerFactory ?? throw new ArgumentNullException(nameof(deviceControllerFactory));
            _workerController = workerController;

            //todo for what?
           // Session = new SessionModel();
            _logger = logger;
        }

        /// <summary>
        /// Обрабатывает нажатие на кнопку старт/пауза
        /// </summary>
        private async Task StartButtonClickAsync()
        {
            switch (SessionStatus)
            {
                case SessionStatus.InProgress:
                    await PauseAsync().ConfigureAwait(true);
                    break;
                case SessionStatus.Suspended:
                    await ResumeAsync().ConfigureAwait(true);
                    break;
                default:
                    _isResultSaved = false;
                    var bedInitParams =
                        _deviceControllerFactory.CreateBedControllerInitParams(_maxAngle, _repeatCount, _frequency);
                    var monitorInitParams = _deviceControllerFactory.CreateMonitorControllerInitParams();

                    var startParams = new SessionParams(
                        _repeatCount,
                        //todo в параметры
                        TimeSpan.FromMilliseconds(300),
                        bedInitParams,
                        monitorInitParams,
                        PumpingNumberOfAttemptsOnStartAndFinish,
                        PumpingNumberOfAttemptsOnProcessing,
                        _deviceControllerFactory.GetDeviceReconnectionTimeout());

                    var bedController = _deviceControllerFactory.CreateBedController();
                    var monitorController = _deviceControllerFactory.CreateMonitorController();
                    Init(
                        startParams,
                        bedController,
                        monitorController,
                        _workerController);
                    await StartAsync().ConfigureAwait(true);
                    break;
            }
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
//                _sessionsService.Add(Session.Session);
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
        private async void ReverseButtonClickAsync()
        {
            await ReverseAsync().ConfigureAwait(true);

        }

        /// <summary>
        /// Прерывает сеанс
        /// </summary>
        private async void EmergencyStopButtonClickAsync()
        {
            await EmeregencyStopAsync().ConfigureAwait(true);
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
            throw new NotImplementedException();
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

            return true;
        }

        public async Task CloseAsync()
        {
            await EmeregencyStopAsync();
        }

        public event Func<object, Task> PageCanceled;
        public event Func<object, Task> PageCompleted;
        public event Func<object, Task> PageBackRequested;
        public event Func<object, TransitionRequest, Task> PageTransitionRequested;
    }

    #endregion
}
