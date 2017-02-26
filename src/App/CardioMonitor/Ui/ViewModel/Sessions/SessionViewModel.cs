﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Monitor;
using CardioMonitor.Files;
using CardioMonitor.Resources;
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

        private readonly IReadOnlyList<double> _checkPointsAngles; 

        /// <summary>
        /// Таймаут запроса параметром пациента
        /// </summary>
        private readonly TimeSpan _updatePatientParamTimeout;

        private readonly TimeSpan _pumpingTimeout;
        
        private readonly TimeSpan _angleUpdateTimeout;
        
        /// <summary>
        /// Точность для сравнение double величин
        /// </summary>
        private const double Tolerance = 0.1e-12;

        /// <summary>
        /// Специальная велична, необходимая для корректной работы определения разрешения
        /// </summary>
        /// <remarks>
        /// Без нее все плохо, слишком часто вызывается метод
        /// </remarks>
        private const double ResolutionToleranceAgnle = 6;

        /// <summary>
        /// Скорость подъема кровати
        /// </summary>
        /// <remarks>
        /// Эмуляция работы железа
        /// </remarks>
        private const double UpAnglePerSecond = 0.6;

        /// <summary>
        /// Скорость спуска кровати
        /// </summary>
        /// <remarks>
        /// Эмуляция работы железа
        /// </remarks>
        private const double DownAnglePerSecond = 0.3;

        /// <summary>
        /// Длительность одного периода в цикле
        /// </summary>
        /// <remarks>
        /// Эмуляция работы железа
        /// </remarks>
        private const int PeriodDuration = 5;

        /// <summary>
        /// Количество периодов в одном цикле
        /// </summary>
        /// <remarks>
        /// Эмуляция работы железа
        /// </remarks>
        private const int PeriodsCount = 6;

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
        private int _periodSesonds;
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
        private AutoPumpingResolver _autoPumpingResolver;
        private bool _startBedFlag = false;
        private readonly object _passedCheckPointsAnglesLockObject;

        private double _previuosCheckPointAngle;
        private readonly IBedController _bedUsbController;

        private readonly TaskHelper _taskHelper;
        private readonly ILogger _logger;
        private readonly IFilesManager _filesRepository;
        private readonly ISessionsRepository _sessionsRepository;
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

                   // _currentAngle = value > 0 ? value : 0;
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

        #region Эмуляция работы железа

        /// <summary>
        /// Прошедшие секунды одного периода
        /// </summary>
        /// <remarks>5 секунд на один период</remarks>
        private int PeriodSeconds
        {
            get { return _periodSesonds; }
            set
            {
                if (value > PeriodDuration)
                {
                    _periodSesonds = 1;
                    PeriodNumber++;
                }
                else
                {
                    _periodSesonds = value;
                }
            }
        }

        /// <summary>
        /// Номер периода
        /// </summary>
        /// <remarks>1 из 6</remarks>
        private int PeriodNumber
        {
            get { return _periodNumber; }
            set
            {
                if (value > PeriodsCount)
                {
                    _periodNumber = 1;
                    if (_isNeedReversing && !_isReversing)
                    {
                        _isUpping = false;
                        _isNeedReversing = false;
                        _isReversing = true;
                        _mainTimer.Stop();
                       /* RemainingTime = (ElapsedTime.Minutes > 1) 
                                        ? ElapsedTime - new TimeSpan(0, 0, 30) 
                                        : ElapsedTime;*/
                        RemainingTime = ElapsedTime;
                        _mainTimer = new CardioTimer(TimerTick, RemainingTime, new TimeSpan(0, 0, 0, 1));
                        _mainTimer.Start();
                    }
                }
                else
                {
                    _periodNumber = value;
                }
            }
        }

        #endregion

        /// <summary>
        /// Помощник потоков для выполнения фунциий в потоке GUI
        /// </summary>
        public ThreadAssistant ThreadAssistant { get; set; }

        private ObservableCollection<DataPoint> _points;
        public ObservableCollection<DataPoint> Points
        {
            get { return _points; }
            set
            {
                _points = value;
                RisePropertyChanged("Points");
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

        /// <summary>
        /// ViewModel для сеанса
        /// </summary>
        public SessionViewModel(
            ILogger logger,
            IFilesManager filesRepository,
            ISessionsRepository sessionsRepository,
            IDeviceControllerFactory deviceControllerFactory,
            TaskHelper taskHelper)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (filesRepository == null) throw new ArgumentNullException(nameof(filesRepository));
            if (sessionsRepository == null) throw new ArgumentNullException(nameof(sessionsRepository));
            if (deviceControllerFactory == null) throw new ArgumentNullException(nameof(deviceControllerFactory));
            if (taskHelper == null) throw new ArgumentNullException(nameof(taskHelper));

            _logger = logger;
            _filesRepository = filesRepository;
            _sessionsRepository = sessionsRepository;
            _deviceControllerFactory = deviceControllerFactory;

            Session = new SessionModel();
            _oneSecond = new TimeSpan(0,0,0,1);
            _isNeedReversing = false;
            _isReversing = false;
            _halfSessionTime = new TimeSpan(0,0,10,0);
            _updatePatientParamTimeout = new TimeSpan(0,0,8);
            _pumpingTimeout = new TimeSpan(0, 0, 8);
            _angleUpdateTimeout = new TimeSpan(0, 0, 1);
            _passedCheckPointsAnglesLockObject = new object();
            // Контрольные точки
            _checkPointsAngles = new List<double> {0, 10.5, 21, 30};
            _bedUsbController = _deviceControllerFactory.CreateBedController();

            _taskHelper = taskHelper;
            _logger = logger;
            _autoPumpingResolver = new AutoPumpingResolver(_logger);
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
                PeriodSeconds = 0;
                PeriodNumber = 1;
                _previuosCheckPointAngle = -10;

                ExecutionStatus = "Старт...";
                var isPumpingFailed = false;

                var progressController =
                await MessageHelper.Instance.ShowProgressDialogAsync("Подождите, идет накачка давления");
                try
                {

                    var pumpingTask = _monitorController.PumpCuffAsync();
                    //на посылку команды накачки выделяем 5 секунд
                    var pumpingResult = await _taskHelper.StartWithTimeout(pumpingTask, _pumpingTimeout);
                    
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

                var currentAngle = CurrentAngle;
                if (IsNeedUpdateData(currentAngle))
                {
                    UpdateData(currentAngle);
                }

                Points = new ObservableCollection<DataPoint>
                {
                    new DataPoint(_x++, _y++),
                    new DataPoint(_x++, _y++),
                    new DataPoint(_x++, _y++),
                    new DataPoint(_x++, _y++),
                    new DataPoint(_x++, _y++),
                    new DataPoint(_x++, _y++),
                };
                
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
            PeriodSeconds++;
            UpdateAngle();
            var currentAngle = CurrentAngle;
            //Накачка давления при необходимости
            if (_autoPumpingResolver.NeedPumping(currentAngle, _isUpping))
            {
                Pump();
            }

            //По идеи уже здесь должно гарантироваться, что только один поток запустит обновление
            if (IsNeedUpdateData(currentAngle))
            {
                UpdateData(currentAngle);
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

        private void Pump()
        {
            Task.Factory.StartNew(() =>
            {
                //ExecutionStatus = "Выполняется накачка кровати...";
                //Мы берем задачу накачки, но пока не запускае ее.
                try
                {
                    var pumpingTask = _monitorController.PumpCuffAsync();
                    var pumpingResult = _taskHelper.StartWithTimeout(pumpingTask, _pumpingTimeout);
                    ExecutionStatus = String.Empty;


                    if (!pumpingResult.Result)
                    {
                        // Нужно решить, что делать, если не удалось провести накачку
                        // Пока так
                        // EmergencyStop();
                         MessageHelper.Instance.ShowMessageAsync("Не удалось провести накачку");
                    }
                }
                catch (TimeoutException)
                {
                    //если бесконечено соединение
                }
                catch (Exception)
                {
                    //на остальные случаи
                    //TODO Вообще сюда надо будет добавить обработчики и логирование
                }
            });
        }

        /// <summary>
        /// Обновляет угол наклона
        /// </summary>
        /// <remarks>
        /// Эмуляция работы железа
        /// </remarks>
        private async void UpdateAngle()
        {
#if Debug_No_Monitor
            if (1 == PeriodNumber)
            {
                CurrentAngle += _isUpping ? UpAnglePerSecond : (-1)*UpAnglePerSecond;
            }
            if (2 == PeriodNumber)
            {
                CurrentAngle -= _isUpping ? DownAnglePerSecond : (-1) * DownAnglePerSecond;
            }
#else
           CurrentAngle = await _bedUsbController.GetAngleXAsync(); 
        /*   if (1 == PeriodNumber)
           {
               CurrentAngle += _isUpping ? UpAnglePerSecond : (-1) * UpAnglePerSecond;
           }
           if (2 == PeriodNumber)
           {
               CurrentAngle -= _isUpping ? DownAnglePerSecond : (-1) * DownAnglePerSecond;
           }*/
          
#endif
        }

        /// <summary>
        /// Проверяет, есть ли необходимость запросить данные в данном угле
        /// </summary>
        /// <param name="currentAngle">Текущий угол</param>
        /// <returns>Разрешение обновления</returns>
        private bool IsNeedUpdateData(double currentAngle)
        {
            lock (_passedCheckPointsAnglesLockObject)
            {
                if (_checkPointsAngles == null) { return false; }

                foreach (var checkPointAngle in _checkPointsAngles)
                {
                    if (Math.Abs(currentAngle - checkPointAngle) < Tolerance)
                    {
                        if (Math.Abs(_previuosCheckPointAngle - checkPointAngle) < ResolutionToleranceAgnle) { return false; }

                        _previuosCheckPointAngle = checkPointAngle;
                        return true;
                    }
                }
                return false;
            }
        }
        
        /// <summary>
        /// Обновляет данные
        /// </summary>
        private async void UpdateData(double currentAngle)
        {
            PatientParams param;
            try
            {
                var gettingParamsTask = _monitorController.GetPatientParamsAsync();
                param = await _taskHelper.StartWithTimeout(gettingParamsTask, _updatePatientParamTimeout);
            }
            catch (TimeoutException)
            {
                param = new PatientParams
                {
                    RepsirationRate = -1,
                    HeartRate = -1
                };
            }
            //TODO по-хорошему, надо предусмотреть обработку и других исключений 
            param.InclinationAngle = Math.Abs(currentAngle) < Tolerance ? 0 : currentAngle;
            ThreadAssistant.StartInUiThread(() => Session.PatientParams.Add(param));
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
                _sessionsRepository.AddSession(Session.Session.ToEntity());
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
                _autoPumpingResolver = new AutoPumpingResolver(_logger);
                _previuosCheckPointAngle = 30;
                
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
            _autoPumpingResolver = new AutoPumpingResolver(_logger);
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
                _previuosCheckPointAngle = 30;
                _autoPumpingResolver = new AutoPumpingResolver(_logger);
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