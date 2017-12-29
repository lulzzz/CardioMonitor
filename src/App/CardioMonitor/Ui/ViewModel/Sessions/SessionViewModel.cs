using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.CoreContracts.Treatment;
using CardioMonitor.BLL.SessionProcessing.Processing;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Monitor;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Files;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Infrastructure.Threading;
using CardioMonitor.Resources;
using CardioMonitor.BLL.SessionProcessing;
using CardioMonitor.SessionProcessing;
using CardioMonitor.Threading;
using CardioMonitor.Ui.Base;

//using CardioMonitor.Core.Repository.Controller;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    /// <summary>
    /// ViewModel для сеанса
    /// </summary>
    public class SessionViewModel : Notifier, IViewModel
    {
        #region Константы

        /// <summary>
        /// Точность для сравнение double величин
        /// </summary>
        private const double Tolerance = 0.1e-12;

        

        #endregion

        #region Поля


        private readonly TimeSpan _oneSecond;
        private readonly TimeSpan _halfSessionTime;

        private Patient _patient;
        private SessionModel _session;
        private double _currentAngle;
        private TimeSpan _elapsedTime;
        private TimeSpan _remainingTime;
        private int _repeatCount;
        private string _startButtonText;
        private int _periodSeconds;
        private int _periodNumber;
        private bool _isUpping;
        private bool _isNeedReversing;
        private bool _isReversing;
        private BedStatus _bedStatus;
        private ICommand _startCommand;
        private ICommand _reverseCommand;
        private ICommand _emergencyStopCommand;

        private CardioTimer _mainTimer;
        private CardioTimer _checkStatusTimer;
        private PumpingResolver _pumpingResolver;
        private bool _startBedFlag = false;

        private readonly IBedController _bedUsbController;

        private readonly TaskHelper _taskHelper;
        private readonly ILogger _logger;
        private readonly IFilesManager _filesRepository;
        private readonly ISessionsService _sessionsService;
        private readonly IDeviceControllerFactory _deviceControllerFactory;

        private readonly IMonitorController _monitorController;
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
                    RisePropertyChanged("PatientEntity");
                    RisePropertyChanged("Patients");
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

        /// <summary>
        /// Сеанс
        /// </summary>
        public SessionModel Session
        {
            get { return _session; }
            private set
            {
                if (!value.Equals(_session))
                {
                    _session = value;
                    RisePropertyChanged("Session");
                    RisePropertyChanged("Status");
                    ElapsedTime = new TimeSpan();
                    RemainingTime = new TimeSpan();
                    StartButtonText = Localisation.SessionViewModel_StartButton_Text;
                }
            }
        }

        /// <summary>
        /// Текущий угол наклона кровати
        /// </summary>
        public double CurrentAngle
        {
            get { return _currentAngle; }
            set
            {
                if (Math.Abs(value - _currentAngle) > Tolerance)
                {
                    _currentAngle = value;
                    RisePropertyChanged("CurrentAngle");
                }
            }
        }

        /// <summary>
        /// Прошедшее время
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
            set
            {
                if (value != _elapsedTime)
                {
                    _elapsedTime = value;
                    RisePropertyChanged("ElapsedTime");
                }
            }
        }

        /// <summary>
        /// Оставшееся время
        /// </summary>
        public TimeSpan RemainingTime
        {
            get { return _remainingTime; }
            set
            {
                if (value != _remainingTime)
                {
                    _remainingTime = value;
                    RisePropertyChanged("RemainingTime");
                }
            }
        }

        /// <summary>
        /// Количество повторений сеанса
        /// </summary>
        public int RepeatCount
        {
            get { return _repeatCount; }
            set
            {
                if (value != _repeatCount)
                {
                    _repeatCount = value;
                    RisePropertyChanged("RepeatCount");
                }
            }
        }

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
                    RisePropertyChanged("StartButtonText");
                }
            }
        }

        /// <summary>
        /// Статус сеанса
        /// </summary>
        public SessionStatus Status
        {
            get { return _session.Status; }
            set
            {
                if (value != _session.Status)
                {
                    _session.Status = value;
                    RisePropertyChanged("Session");
                    RisePropertyChanged("Status");
                }
            }
        }
        /// <summary>
        /// Статус кровати
        /// </summary>
        public BedStatus BedStatus
        {
            get { return _bedStatus; }
            set
            {
                if (!Equals(value, _bedStatus))
                {
                    _bedStatus = value;
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
                    CanExecuteDelegate = o => SessionStatus.Terminated != Status && SessionStatus.Completed != Status,
                    ExecuteDelegate = o => StartSessionButtonClick()
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
                    CanExecuteDelegate = o => SessionStatus.InProgress == Status && (!_isReversing && !_isNeedReversing) && ElapsedTime < _halfSessionTime,
                    ExecuteDelegate = o => Reverse()
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
                    CanExecuteDelegate = o => SessionStatus.InProgress == Status || SessionStatus.Suspended == Status,
                    ExecuteDelegate = o => EmergencyStop()
                });
            }
        }

        /// <summary>
        /// Курс лечения
        /// </summary>
        public Treatment Treatment { get; set; }

        public EventHandler UpdateEcg { get; set; }

        private bool _needUpdate;

        public bool NeedUpdate
        {
            get { return _needUpdate; }
            set
            {
                _needUpdate = value;
                RisePropertyChanged("NeedUpdate");
            }
        }

        public bool IsConnected { get; set; }
        
        /// <summary>
        /// Помощник потоков для выполнения фунциий в потоке GUI
        /// </summary>
        public ThreadAssistant ThreadAssistant { get; set; }

        //private ObservableCollection<DataPoint> _points;
        //public ObservableCollection<DataPoint> Points
        //{
        //    get { return _points; }
        //    set
        //    {
        //        _points = value;
        //        RisePropertyChanged("Points");
        //    }
        //}

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
        public SessionViewModel(
            ILogger logger,
            IFilesManager filesRepository,
            ISessionsService sessionsService,
            IDeviceControllerFactory deviceControllerFactory,
            TaskHelper taskHelper)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (filesRepository == null) throw new ArgumentNullException(nameof(filesRepository));
            if (sessionsService == null) throw new ArgumentNullException(nameof(sessionsService));
            if (deviceControllerFactory == null) throw new ArgumentNullException(nameof(deviceControllerFactory));
            if (taskHelper == null) throw new ArgumentNullException(nameof(taskHelper));

            _logger = logger;
            _filesRepository = filesRepository;
            _sessionsService = sessionsService;
            _deviceControllerFactory = deviceControllerFactory;

            Session = new SessionModel();
            _oneSecond = new TimeSpan(0,0,0,1);
            _isNeedReversing = false;
            _isReversing = false;
            _halfSessionTime = new TimeSpan(0,0,10,0);
            _bedUsbController = _deviceControllerFactory.CreateBedController();

            _taskHelper = taskHelper;
            _logger = logger;
            _pumpingResolver = new PumpingResolver(_logger);
            _monitorController = deviceControllerFactory.CreateMonitorController();
        }

        /// <summary>
        /// Обрабатывает нажатие на кнопку старт/пауза
        /// </summary>
        private void StartSessionButtonClick()
        {
            switch (Session.Status)
            {
                case SessionStatus.InProgress:
                    SuspendSesion();
                    break;
                case SessionStatus.Suspended:
                    ResumeSession();
                    break;
                default:
                    SessionStart();
                    break;
            }
        }

        /// <summary>
        /// Запускает сеанс
        /// </summary>
        private async void SessionStart()
        {
            var cycles = new List<SessionCycleViewModel>();
            for (int i = 0; i < RepeatCount; i++)
            {
                cycles.Add(new SessionCycleViewModel());
            }
            Session.Cycles = new ObservableCollection<SessionCycleViewModel>(cycles);
            BedStatus = _bedUsbController.GetBedStatus();
            if (BedStatus == BedStatus.Ready)
            {
                StartButtonText = Localisation.SessionViewModel_PauseButton_Text;
                Session.Status = SessionStatus.InProgress;
                Session.TreatmentId = Treatment.Id;
                Session.DateTime = DateTime.Now;
                ElapsedTime = new TimeSpan(0, 0, 0, 0);
                RemainingTime = new TimeSpan(0, 0, 20, 0);
                CurrentAngle = 0;

                ExecutionStatus = "Старт...";
                var isPumpingFailed = false;

                var progressController =
                await MessageHelper.Instance.ShowProgressDialogAsync("Подождите, идет накачка давления");
                try
                {

                    var pumpingTask = _monitorController.PumpCuffAsync();
                    //на посылку команды накачки выделяем 5 секунд
                    var pumpingResult = await _taskHelper.StartWithTimeout(pumpingTask, /*_pumpingTimeout*/new TimeSpan(0, 0, 5));
                    
                    // просто ожидаем 60 скеунд
                    await Task.Delay(new TimeSpan(0,0,60));

                    await progressController.CloseAsync();

                    // Если накачка прошла неуспешно
                    if (!pumpingResult)
                    {
                        await MessageHelper.Instance.ShowMessageAsync("Не удалось провести накачку давления.");
                    }
                    

                }
                catch (TimeoutException)
                {
                    isPumpingFailed = true;
                }
                catch (Exception)
                {
                    //TODO и сюда логирование
                }
                if (isPumpingFailed)
                {
                    await progressController.CloseAsync();
                    await MessageHelper.Instance.ShowMessageAsync("Не удалось провести накачку давления.");
                }
                
                _isUpping = true;
                _isNeedReversing = false;
                _isReversing = false;
                _bedUsbController.ExecuteCommand(BedControlCommand.Start);
                //TODO можно протестить все за 30 секунд, раскомментровать следующу строку, закомментровать следующую за ней
               // _mainTimer = new CardioTimer(TimerTick, new TimeSpan(0, 0, 2, 0), new TimeSpan(0, 0, 0, 0, 100));
                _mainTimer = new CardioTimer(TimerTick, new TimeSpan(0, 0, 20, 0), new TimeSpan(0, 0, 0, 1));
                _mainTimer.Start();
                _startBedFlag = true;

                
                //Points = new ObservableCollection<DataPoint>
                //{
                //    new DataPoint(_x++, _y++),
                //    new DataPoint(_x++, _y++),
                //    new DataPoint(_x++, _y++),
                //    new DataPoint(_x++, _y++),
                //    new DataPoint(_x++, _y++),
                //    new DataPoint(_x++, _y++),
                //};
                
            }
            else
            {
                if (BedStatus == BedStatus.Calibrating)
                {
                   await MessageHelper.Instance.ShowMessageAsync("Кровать находится в состоянии калибровки. Дождитесь ее окончания и повторите попытку");
                }
                if (BedStatus == BedStatus.Loop)
                {
                    await MessageHelper.Instance.ShowMessageAsync("Цикл уже запущен. Дождитесь его окончания или экстренно завершите");
                }
                if (BedStatus == BedStatus.NotReady)
                {
                   await  MessageHelper.Instance.ShowMessageAsync("Кровать не готова. Дождитесь окончания подготовки и повторите попытку");
                }
                if (BedStatus == BedStatus.Disconnected)
                {
                    await MessageHelper.Instance.ShowMessageAsync("Кровать не подключена. Проверьте соединение");
                }
            }
        }

        private int _x;
        private int _y;

        /// <summary>
        /// Обработка тика таймера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerTick(object sender, EventArgs e)
        {
            //_mainTimer.Suspend();

            ElapsedTime += _oneSecond;
            RemainingTime -= _oneSecond;
            if (ElapsedTime >= RemainingTime)
            {
                _isUpping = false;
            }
            _periodSeconds++;
            //UpdateAngle(); todo
            var currentAngle = CurrentAngle;
            //Накачка давления при необходимости
            if (_pumpingResolver.NeedPumping(currentAngle, _isUpping))
            {
               // Pump(); todo
            }

           
            
            if (TimeSpan.Zero == RemainingTime)
            {
                ThreadAssistant.StartInUiThread(SessionComplete);
            }
            ThreadAssistant.StartInUiThread(StopTimerAndSaveSession);
            //Для ЭКГ графика
            //ThreadAssistant.StartInUiThread(() =>
            //    Points.Add(new DataPoint(x++, Math.Pow(-1, y)*y++)));

            //NeedUpdate = true;
            //_mainTimer.Resume();
        }
      
        
        /// <summary>
        /// Приостанавливает сеанс
        /// </summary>
        private async void SuspendSesion()
        {
            if (_bedUsbController.IsConnected())
            {
                _bedUsbController.ExecuteCommand(BedControlCommand.Pause);
                _mainTimer.Suspend();
                StartButtonText = Localisation.SessionViewModel_StartButton_Text;
                Session.Status = SessionStatus.Suspended;
               
            }
            else
            {
                await MessageHelper.Instance.ShowMessageAsync("Нет соединения с кроватью");
            }
        }

        /// <summary>
        /// Продолжает сеанс после приостановки
        /// </summary>
        private async void ResumeSession()
        {

            if (_bedUsbController.IsConnected())
            {
                _bedUsbController.ExecuteCommand(BedControlCommand.Start);
                StartButtonText = Localisation.SessionViewModel_PauseButton_Text;
                Session.Status = SessionStatus.InProgress;
                await Task.Delay(new TimeSpan(0, 0, 2));
                _mainTimer.Resume();
                StartButtonText = Localisation.SessionViewModel_PauseButton_Text;
                Session.Status = SessionStatus.InProgress;
                
            }
            else
            {
                await MessageHelper.Instance.ShowMessageAsync("Нет соединения с кроватью");
            }
        }

        /// <summary>
        /// Завершает сенас
        /// </summary>
        private void SessionComplete()
        {
            Session.Status = SessionStatus.Completed;
            SaveSession();
        }

        /// <summary>
        /// Сохраняет результаты сеанса в базу и файл
        /// </summary>
        private async void SaveSession()
        {
            var exceptionMessage = String.Empty;
            try
            {
                _filesRepository.SaveToFile(Patient, Session.Session);
                //todo это работать не будет
                _sessionsService.Add(Session.Session);
                await MessageHelper.Instance.ShowMessageAsync(Localisation.SessionViewModel_SessionCompeted);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError("SessionViewModel", ex);
                exceptionMessage = Localisation.SessionViewModel_SaveSession_ArgumentNullException;
            }
            catch (Exception ex)
            {
                exceptionMessage = ex.Message;
            }
            if (!String.IsNullOrEmpty(exceptionMessage))
            {
                await MessageHelper.Instance.ShowMessageAsync(exceptionMessage);
            }
        }

        /// <summary>
        /// Реверс
        /// </summary>
        private async void Reverse()
        {
            if (_bedUsbController.IsConnected())
            {
                _bedUsbController.ExecuteCommand(BedControlCommand.Reverse);
                _isNeedReversing = true;
                _pumpingResolver = new PumpingResolver(_logger);
                
                //ThreadAssistant.StartInUiThread(() => {  MessageHelper.Instance.ShowMessageAsync("Запущен реверс"); });
               // await MessageHelper.Instance.ShowMessageAsync("Запущен реверс");
            }
            else
            {
                await MessageHelper.Instance.ShowMessageAsync("Нет соединения с кроватью");
            }
        }

        /// <summary>
        /// Прерывает сеанс
        /// </summary>
        private async void EmergencyStop()
        {
            if (_bedUsbController.IsConnected())
            {
                _bedUsbController.ExecuteCommand(BedControlCommand.EmergencyStop);
                _mainTimer.Stop();
                Session.Status = SessionStatus.Terminated;
                SaveSession();
                _startBedFlag = false;
                

            }
            else
            {
                await MessageHelper.Instance.ShowMessageAsync("Нет соединения с кроватью");
            }
        }

        /// <summary>
        /// Очищает содержимое ViewModel, обнуляя все данные
        /// </summary>
        public void Clear()
        {
            Patient = null;
            CurrentAngle = 0;
            RemainingTime = TimeSpan.Zero;
            ElapsedTime = TimeSpan.Zero;
            Status = SessionStatus.Unknown;
            Session = new SessionModel();
            if (_mainTimer != null) {_mainTimer.Stop();}
            if (_checkStatusTimer != null) { _checkStatusTimer.Stop(); }
            _pumpingResolver = new PumpingResolver(_logger);
            _startBedFlag = false;
        }
        public void StartStatusTimer()
        {
            _checkStatusTimer = new CardioTimer(StatusTimerTick, new TimeSpan(10, 0, 0, 0), new TimeSpan(0, 0, 0, 1));
            _checkStatusTimer.Start();
        }

        public void StopTimerAndSaveSession()
        {
            if ((Status == SessionStatus.InProgress)&&(CurrentAngle == 0)&&(RemainingTime < ElapsedTime)&&(BedStatus == BedStatus.Ready))
            {
                SessionComplete();
                _startBedFlag = false;
                RemainingTime = TimeSpan.Zero;
                ElapsedTime = TimeSpan.Zero;
                if (_mainTimer != null) { _mainTimer.Stop(); }
                if (_checkStatusTimer != null) { _checkStatusTimer.Stop(); }
            }
        }

        private async void StatusTimerTick(object sender, EventArgs e)
        {
           
            BedStatus = _bedUsbController.GetBedStatus();
            _bedUsbController.UpdateFlags();
           /* if ((BedStatus == BedStatus.Loop) && (_bedUsbController.StartFlag == StartFlag.Start) && (Session.Status == SessionStatus.Unknown) && (!_startBedFlag))
            {
                _previuosCheckPointAngle = -10;
                StartButtonText = Localisation.SessionViewModel_PauseButton_Text;
                Session.Status = SessionStatus.InProgress;
                Session.TreatmentId = Treatment.Id;
                Session.DateTime = DateTime.Now;
                ElapsedTime = new TimeSpan(0, 0, 0, 0);
                RemainingTime = new TimeSpan(0, 0, 18, 0);
                CurrentAngle = 0;
                PeriodSeconds = 0;
                PeriodNumber = 1;
                ExecutionStatus = "Старт..."; 
                _isUpping = true;
                _isNeedReversing = false;
                _isReversing = false;
                _mainTimer = new CardioTimer(TimerTick, new TimeSpan(0, 0, 18, 0), new TimeSpan(0, 0, 0, 1));
                _mainTimer.Start();
                var currentAngle = CurrentAngle;
                if (IsNeedUpdateData(currentAngle))
                {
                    UpdateData(currentAngle);
                }
            }*/

            if ((BedStatus == BedStatus.Loop) && (_bedUsbController.GetStartFlag() == StartFlag.Pause) && (Session.Status == SessionStatus.InProgress))
            {
                Session.Status = SessionStatus.Suspended;
                _mainTimer.Suspend();
                StartButtonText = Localisation.SessionViewModel_StartButton_Text;
                ThreadAssistant.StartInUiThread(() => { MessageHelper.Instance.ShowMessageAsync("Нажата пауза"); });
            }
            //если запущен цикл, кровать была на паузе и была выведена из нее - продолжить работу
            if ((BedStatus == BedStatus.Loop) && (_bedUsbController.GetStartFlag() == StartFlag.Start) && (Session.Status == SessionStatus.Suspended))
            {
                Session.Status = SessionStatus.InProgress;
                StartButtonText = Localisation.SessionViewModel_PauseButton_Text;
                _mainTimer.Resume();
                ThreadAssistant.StartInUiThread(() => { MessageHelper.Instance.ShowMessageAsync("Нажат старт"); });
            }
            //если кровать запущена и пришел флаг реверса - реверс
            
            //TODO исправить этом при первом случае
            // здесь вы найдете ебучий костыль 
            if ((BedStatus == BedStatus.Loop) && (_bedUsbController.GetReverseFlag() == ReverseFlag.Reversed) && (Session.Status == SessionStatus.InProgress)
                && (!_isNeedReversing) && (!_isNeedReversing) && (!_isNeedReversing) && (!_isNeedReversing) && (!_isNeedReversing))
            {
                _isNeedReversing = true;
                _pumpingResolver = new PumpingResolver(_logger);
               // await MessageHelper.Instance.ShowMessageAsync("Запущен реверс");
                ThreadAssistant.StartInUiThread(() => { MessageHelper.Instance.ShowMessageAsync("Запущен реверс"); });
            }


            //если кровать запущена и пришел флаг экстренной остановки - завершить сеанс
            if ((BedStatus == BedStatus.NotReady) && ((Session.Status == SessionStatus.InProgress) || (Session.Status == SessionStatus.Suspended)))
            {
                Session.Status = SessionStatus.Terminated;
                _mainTimer.Stop();
                ThreadAssistant.StartInUiThread(SaveSession);
                _startBedFlag = false;
            }
            CommandManager.InvalidateRequerySuggested();

            
        }
    }
}
