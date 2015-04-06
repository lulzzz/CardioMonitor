﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CardioMonitor.Core;
using CardioMonitor.Core.Models.Patients;
using CardioMonitor.Core.Models.Session;
using CardioMonitor.Core.Models.Treatment;
using CardioMonitor.Core.Repository.DataBase;
using CardioMonitor.Core.Repository.Files;
using CardioMonitor.Core.Repository.Monitor;
using CardioMonitor.Logs;
using CardioMonitor.Resources;
using CardioMonitor.ViewModel.Base;

namespace CardioMonitor.ViewModel.Sessions
{
    /// <summary>
    /// ViewModel для сеанса
    /// </summary>
    public class SessionViewModel : Notifier, IViewModel
    {
        #region Поля

        private const double Tolerance = 0.1e-12;

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

        private ICommand _startCommand;
        private ICommand _reverseCommand;
        private ICommand _emergencyStopCommand;

        private CardioTimer _mainTimer;

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
                    RisePropertyChanged("Patient");
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
                if (value != _session)
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

                    _currentAngle = value > 0 ? value : 0;
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
        /// Команда старта/пауза сеанса
        /// </summary>
        public ICommand StartCommand
        {
            get
            {
                return _startCommand ?? (_startCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => SessionStatus.Terminated != Status && SessionStatus.Completed != Status,
                    ExecuteDelegate = x => StartSessionButtonClick()
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
                    CanExecuteDelegate = x => SessionStatus.InProgress == Status && (!_isReversing && !_isNeedReversing) && ElapsedTime < _halfSessionTime,
                    ExecuteDelegate = x => Reverse()
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
                    CanExecuteDelegate = x => SessionStatus.InProgress == Status || SessionStatus.Suspended == Status,
                    ExecuteDelegate = x => EmergencyStop()
                });
            }
        }

        /// <summary>
        /// Курс лечения
        /// </summary>
        public Treatment Treatment { get; set; }

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

        #endregion

        /// <summary>
        /// ViewModel для сеанса
        /// </summary>
        public SessionViewModel()
        {
            Session = new SessionModel();
            _oneSecond = new TimeSpan(0,0,0,1);
            _isNeedReversing = false;
            _isReversing = false;
            _halfSessionTime = new TimeSpan(0,0,10,0);
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
        private void SessionStart()
        {
            StartButtonText = Localisation.SessionViewModel_PauseButton_Text;
            Session.Status = SessionStatus.InProgress;
            Session.TreatmentId = Treatment.Id;
            Session.DateTime = DateTime.Now;
            ElapsedTime = new TimeSpan(0,0,0,0);
            RemainingTime = new TimeSpan(0, 0, 20, 0);
            CurrentAngle = 0;
            PeriodSeconds = 0;
            PeriodNumber = 1;

            _isUpping = true;
            _isNeedReversing = false;
            _isReversing = false;

            //TODO можно протестить все за 30 секунд, раскомментровать следующу строку, закомментровать следующую за ней
            _mainTimer = new CardioTimer(TimerTick, new TimeSpan(0, 0, 0, 30), new TimeSpan(0,0,0,0,25));
           // _mainTimer = new CardioTimer(TimerTick, new TimeSpan(0, 0, 20, 0), new TimeSpan(0, 0, 0, 1));
            _mainTimer.Start();
            UpdateData();
        }

        /// <summary>
        /// Обработка тика таймера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerTick(object sender, EventArgs e)
        {
            ElapsedTime += _oneSecond;
            RemainingTime -= _oneSecond;
            if (ElapsedTime == RemainingTime)
            {
                _isUpping = false;
            }
            PeriodSeconds++;
            UpdateAngle();
            UpdateData();
            if (TimeSpan.Zero == RemainingTime)
            {
                ThreadAssistant.StartInUIThread(SessionComplete);
            }
        }

        /// <summary>
        /// Обновляет угол наклона
        /// </summary>
        /// <remarks>
        /// Эмуляция работы железа
        /// </remarks>
        private void UpdateAngle()
        {
            if (1 == PeriodNumber)
            {
                CurrentAngle += _isUpping ? UpAnglePerSecond : (-1)*UpAnglePerSecond;
            }
            if (2 == PeriodNumber)
            {
                CurrentAngle -= _isUpping ? DownAnglePerSecond : (-1) * DownAnglePerSecond;
            }
        }

        /// <summary>
        /// Обновляет данные
        /// </summary>
        private void UpdateData()
        {
            if ((Math.Abs(CurrentAngle) < Tolerance) || (Math.Abs(CurrentAngle - 10.5) < Tolerance) 
                || (Math.Abs(CurrentAngle - 21) < Tolerance) || (Math.Abs(CurrentAngle - 30) < Tolerance))
            {
                var param = Session.PatientParams.LastOrDefault();
                if (null == param || Math.Abs(CurrentAngle - param.InclinationAngle) > Tolerance)
                {
                    param = MonitorRepository.Instance.GetPatientParams();
                    param.InclinationAngle = Math.Abs(CurrentAngle) < Tolerance ? 0 : CurrentAngle;
                    ThreadAssistant.StartInUIThread(() => Session.PatientParams.Add(param));
                }
            }
        }

        /// <summary>
        /// Приостанавливает сеанс
        /// </summary>
        private void SuspendSesion()
        {
            _mainTimer.Suspend();
            StartButtonText = Localisation.SessionViewModel_StartButton_Text;
            Session.Status = SessionStatus.Suspended;
        }

        /// <summary>
        /// Продолжает сеанс после приостановки
        /// </summary>
        private void ResumeSession()
        {
            _mainTimer.Resume();
            StartButtonText = Localisation.SessionViewModel_PauseButton_Text;
            Session.Status = SessionStatus.InProgress;
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
                FileRepository.SaveToFile(Patient, Session.Session);
                DataBaseRepository.Instance.AddSession(Session.Session);
                await MessageHelper.Instance.ShowMessageAsync(Localisation.SessionViewModel_SessionCompeted);
            }
            catch (ArgumentNullException ex)
            {
                Logger.Instance.LogError("SessionViewModel", ex);
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
        private void Reverse()
        {
            _isNeedReversing = true;
        }

        /// <summary>
        /// Прерывает сеанс
        /// </summary>
        private void EmergencyStop()
        {
            _mainTimer.Stop();
            Session.Status = SessionStatus.Terminated;
            SaveSession();
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
        }
    }
}