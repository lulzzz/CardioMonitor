using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.Core;

namespace CardioMonitor.Patients.Session
{
    public class SessionViewModel : Notifier, IViewModel
    {
        private const double Tolerance = 0.001;

        private Patient _patient;
        private SessionModel _session;
        private double _currentAngle;
        private DateTime _currentTime;
        private DateTime _remainingTime;
        private int _repeatCount;

        private ICommand _startCommand;
        private ICommand _reverseCommand;
        private ICommand _emergencyStopCommand;

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
            get { return new ObservableCollection<Patient>() {Patient} ;}
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
                }
            }
        }

        public double CurreneAngle
        {
            get { return _currentAngle; }
            set
            {;
                if (Math.Abs(value - _currentAngle) > Tolerance)
                {
                    _currentAngle = value;
                    RisePropertyChanged("CurreneAngle");
                }
            }
        }

        public DateTime CurrentTime
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

        public DateTime RemainingTime
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

        public SessionStatus Status
        {
            get { return _session.Status; }
        }

        public ICommand StartCommand
        {
            get
            {
                return _startCommand ?? (_startCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => SessionStatus.Terminated !=  _session.Status,
                    ExecuteDelegate = x => StartSession()
                });
            }
        }

        public ICommand ReverseCommand
        {
            get
            {
                return _reverseCommand ?? (_reverseCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => SessionStatus.InProgress == _session.Status,
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
                    CanExecuteDelegate = x => SessionStatus.InProgress == _session.Status,
                    ExecuteDelegate = x => EmergencyStop()
                });
            }
        }

        public SessionViewModel()
        {
            Session = new SessionModel();
        }

        private async void StartSession()
        {
            await MessageHelper.Instance.ShowMessageAsync("Start");
            if (SessionStatus.InProgress != _session.Status)
            {
                _session.Status = SessionStatus.InProgress;
                Session.PatientParams.Add(new PatientParams
                {
                    AverageArterialPressure = 100,
                    DiastolicArterialPressure = 80,
                    HeartRate = 70,
                    InclinationAngle = 0,
                    RepsirationRate = 40,
                    Spo2 = 40,
                    SystolicArterialPressure = 120
                });
                Session.PatientParams.Add(new PatientParams
                {
                    AverageArterialPressure = 100,
                    DiastolicArterialPressure = 80,
                    HeartRate = 70,
                    InclinationAngle = 10.5,
                    RepsirationRate = 40,
                    Spo2 = 40,
                    SystolicArterialPressure = 120
                });
                Session.PatientParams.Add(new PatientParams
                {
                    AverageArterialPressure = 100,
                    DiastolicArterialPressure = 80,
                    HeartRate = 70,
                    InclinationAngle = 21,
                    RepsirationRate = 40,
                    Spo2 = 40,
                    SystolicArterialPressure = 120
                });
                Session.PatientParams.Add(new PatientParams
                {
                    AverageArterialPressure = 100,
                    DiastolicArterialPressure = 80,
                    HeartRate = 70,
                    InclinationAngle = 30,
                    RepsirationRate = 40,
                    Spo2 = 40,
                    SystolicArterialPressure = 120
                });
                Session.PatientParams.Add(new PatientParams
                {
                    AverageArterialPressure = 100,
                    DiastolicArterialPressure = 80,
                    HeartRate = 70,
                    InclinationAngle = 21,
                    RepsirationRate = 40,
                    Spo2 = 40,
                    SystolicArterialPressure = 120
                });
                Session.PatientParams.Add(new PatientParams
                {
                    AverageArterialPressure = 100,
                    DiastolicArterialPressure = 80,
                    HeartRate = 70,
                    InclinationAngle = 10.5,
                    RepsirationRate = 40,
                    Spo2 = 40,
                    SystolicArterialPressure = 120
                });
                Session.PatientParams.Add(new PatientParams
                {
                    AverageArterialPressure = 100,
                    DiastolicArterialPressure = 80,
                    HeartRate = 70,
                    InclinationAngle = 0,
                    RepsirationRate = 40,
                    Spo2 = 40,
                    SystolicArterialPressure = 120
                });
            }
        }

        private async void Reverse()
        {
            await MessageHelper.Instance.ShowMessageAsync("Reverse");
        }

        private async void EmergencyStop()
        {
            await MessageHelper.Instance.ShowMessageAsync("Stoped");
            _session.Status = SessionStatus.InProgress;
        }

        public void Clear()
        {
            Patient = null;
            Session = new SessionModel();
        }
    }
}
