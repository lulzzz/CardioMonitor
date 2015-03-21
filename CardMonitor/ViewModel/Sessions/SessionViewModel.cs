using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Threading;
using CardioMonitor.Core;
using CardioMonitor.Core.Models.Patients;
using CardioMonitor.Core.Models.Session;
using CardioMonitor.Core.Models.Treatment;
using CardioMonitor.Core.Repository.DataBase;
using CardioMonitor.Core.Repository.Monitor;
using CardioMonitor.ViewModel.Base;
using MahApps.Metro.Controls.Dialogs;

namespace CardioMonitor.ViewModel.Sessions
{
    public class SessionViewModel : Notifier, IViewModel
    {
        private const double Tolerance = 0.1e-12;
        private const double UpAnglePerSecond = 0.6;
        private const double DownAnglePerSecond = 0.3;
        private const int PeriodDuration = 5;
        private const int PeriodsCount = 6;
        private readonly string _startText;
        private readonly string _pauseText;
        private readonly TimeSpan _oneSecond;
        private readonly TimeSpan _halfSessionTime;

        private Patient _patient;
        private SessionModel _session;
        private double _currentAngle;
        private TimeSpan _currentTime;
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

        public ObservableCollection<Patient> Patients
        {
            get { return (null != Patient) 
                            ? new ObservableCollection<Patient> {Patient} 
                            : new ObservableCollection<Patient>();}
        }

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
                    CurrentTime = new TimeSpan();
                    RemainingTime = new TimeSpan();
                    StartButtonText = _startText;
                }
            }
        }

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

        public TimeSpan CurrentTime
        {
            get { return _currentTime; }
            set
            {
                if (value != _currentTime)
                {
                    _currentTime = value;
                    RisePropertyChanged("CurrentTime");
                }
            }
        }

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
        
        public ICommand ReverseCommand
        {
            get
            {
                return _reverseCommand ?? (_reverseCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => SessionStatus.InProgress == Status && (!_isReversing && !_isNeedReversing) && CurrentTime < _halfSessionTime,
                    ExecuteDelegate = x => Reverse()
                });
            }
        }

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

        public Treatment Treatment { get; set; }

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
                        RemainingTime = CurrentTime;
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

        public ThreadAssistant ThreadAssistant { get; set; }

        public SessionViewModel()
        {
            _startText = "Старт";
            _pauseText = "Пауза";
            Session = new SessionModel();
            _oneSecond = new TimeSpan(0,0,0,1);
            _isNeedReversing = false;
            _isReversing = false;
            _halfSessionTime = new TimeSpan(0,0,10,0);
        }

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

        private void SessionStart()
        {
            StartButtonText = _pauseText;
            Session.Status = SessionStatus.InProgress;
            Session.TreatmentId = Treatment.Id;
            Session.DateTime = DateTime.Now;
            CurrentTime = new TimeSpan(0,0,0,0);
            RemainingTime = new TimeSpan(0, 0, 20, 0);
            CurrentAngle = 0;
            PeriodSeconds = 0;
            PeriodNumber = 1;

            _isUpping = true;
            _isNeedReversing = false;
            _isReversing = false;

           // _mainTimer = new CardioTimer(TimerTick, new TimeSpan(0, 0, 0, 30), new TimeSpan(0,0,0,0,25));
            _mainTimer = new CardioTimer(TimerTick, new TimeSpan(0, 0, 20, 0), new TimeSpan(0, 0, 0, 1));
            _mainTimer.Start();
            UpdateData();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            CurrentTime += _oneSecond;
            RemainingTime -= _oneSecond;
            if (CurrentTime == RemainingTime)
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

        private void SuspendSesion()
        {
            _mainTimer.Suspend();
            StartButtonText = _startText;
            Session.Status = SessionStatus.Suspended;
        }

        private void ResumeSession()
        {
            _mainTimer.Resume();
            StartButtonText = _pauseText;
            Session.Status = SessionStatus.InProgress;
        }

        private void SessionComplete()
        {
            Session.Status = SessionStatus.Completed;
            SaveSession();
        }

        private async void SaveSession()
        {
            try
            {
                FileManager.SaveToFile(Patient, Session.Session);
                DataBaseRepository.Instance.AddSession(Session.Session);
                await MessageHelper.Instance.ShowMessageAsync("Сенас завершен.");
            }
            catch (Exception ex)
            {
                MessageHelper.Instance.ShowMessageAsync("Не удалось сохранить результаты сеанса.");
            }
        }

        private async void Reverse()
        {
            _isNeedReversing = true;
        }

        private void EmergencyStop()
        {
            _mainTimer.Stop();
            Session.Status = SessionStatus.Terminated;
            SaveSession();
        }

        public void Clear()
        {
            Patient = null;
            Session = new SessionModel();
        }
    }
}
