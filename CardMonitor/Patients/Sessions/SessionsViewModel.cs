using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.Core;
using CardioMonitor.Patients.Session;
using CardioMonitor.Patients;
using MahApps.Metro.Controls.Dialogs;

namespace CardioMonitor.Patients.Sessions
{
    public class SessionsViewModel : Notifier, IViewModel
    {
        private PatientFullName _patientName;
        private DateTime _treatmentStartDate;
        private SessionInfo _selectedSessionInfo;
        private ObservableCollection<SessionInfo> _sessionInfos;

        private ICommand _startSessionCommand;
        private ICommand _deleteSessionCommand;
        private ICommand _showResultsCommand;

        public PatientFullName PatientName
        {
            get { return _patientName; }
            set
            {
                if (value != _patientName)
                {
                    _patientName = value;
                    RisePropertyChanged("PatientName");
                }
            }
        }

        public DateTime TreatmentStartDate
        {
            get { return _treatmentStartDate; }
            set
            {
                if (value != _treatmentStartDate)
                {
                    _treatmentStartDate = value;
                    RisePropertyChanged("TreatmentStartDate");
                }
            }
        }

        public SessionInfo SelectedSessionInfo
        {
            get { return _selectedSessionInfo; }
            set
            {
                if (value != _selectedSessionInfo)
                {
                    _selectedSessionInfo = value;
                    RisePropertyChanged("SelectedSessionInfo");
                }
            }
        }

        public ObservableCollection<SessionInfo> SessionInfos
        {
            get { return _sessionInfos; }
            set
            {
                if (value != _sessionInfos)
                {
                    _sessionInfos = value;
                    RisePropertyChanged("SessionInfos");
                }
            }
        }

        public ICommand StartSessionCommand
        {
            get
            {
                return _startSessionCommand ?? (_startSessionCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x => StartSession()
                });
            }
        }
        public ICommand DeleteSessionCommand
        {
            get
            {
                return _deleteSessionCommand ?? (_deleteSessionCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => null != SelectedSessionInfo,
                    ExecuteDelegate = x => DeleteSession(x)
                });
            }
        }

        public ICommand ShowResultsCommand
        {
            get
            {
                return _showResultsCommand ?? (_showResultsCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => null != SelectedSessionInfo,
                    ExecuteDelegate = x => ShowResults(x)
                });
            }
        }

        public EventHandler StartSessionEvent { get; set; }

        public EventHandler ShowResultsEvent { get; set; }

        public SessionsViewModel()
        {
            SessionInfos = new ObservableCollection<SessionInfo>
            {
                new SessionInfo {DateTime = new DateTime()},
                new SessionInfo {DateTime = new DateTime()},
                new SessionInfo {DateTime = new DateTime()}
            };
        }

        private void StartSession()
        {
            var handler = StartSessionEvent;
            if (null != handler)
            {
                handler(this, null);
            }
        }

        private async void DeleteSession(object sender)
        {
            var result = await MessageHelper.Instance.ShowMessageAsync("Вы уверены, что хотите удалить сессию?",
                style: MessageDialogStyle.AffirmativeAndNegative);
            if (MessageDialogResult.Affirmative == result)
            {
                var sessionInfo = sender as SessionInfo;
                if (null != sessionInfo)
                {
                    SessionInfos.Remove(sessionInfo);
                }
                else
                {
                    await MessageHelper.Instance.ShowMessageAsync("Не удалось удалить сессию");
                }
            }
        }

        private void ShowResults(object sender)
        {
            var handler = ShowResultsEvent;
            if (null != handler)
            {
                handler(this, null);
            }
        }

        public void Clear()
        {
            TreatmentStartDate = new DateTime();
            SelectedSessionInfo = null;
            SessionInfos = new ObservableCollection<SessionInfo>();
            PatientName = null;
        }
    }
}
