using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.CoreContracts.Treatment;
using CardioMonitor.Resources;
using CardioMonitor.Ui.Base;
using CardioMonitor.Ui.Storyboards;
using MahApps.Metro.Controls.Dialogs;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    public class SessionsViewModel : Notifier//, IStoryboardViewModel
    {
        private readonly ISessionsService _sessionsService;
        private PatientFullName _patientName;
        private DateTime _treatmentStartDate;
        private SessionInfo _selectedSessionInfo;
        private ObservableCollection<SessionInfo> _sessionInfos;

        private ICommand _startSessionCommand;
        private ICommand _deleteSessionCommand;
        private ICommand _showResultsCommand;

        public SessionsViewModel(
            ISessionsService sessionsService)
        {
            if (sessionsService == null) throw new ArgumentNullException(nameof(sessionsService));
            _sessionsService = sessionsService;
        }

        public PatientFullName PatientName
        {
            get { return _patientName; }
            set
            {
                if (value != _patientName)
                {
                    _patientName = value;
                    RisePropertyChanged("PatientEntity");
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

        public Treatment Treatment { get; set; }

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
                    ExecuteDelegate = x => DeleteSession()
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
                    ExecuteDelegate = x => ShowResults()
                });
            }
        }

        public EventHandler StartSessionEvent { get; set; }

        public EventHandler ShowResultsEvent { get; set; }


        private void StartSession()
        {
            var handler = StartSessionEvent;
            handler?.Invoke(this, null);
        }

        private async void DeleteSession()
        {
            var result = await MessageHelper.Instance.ShowMessageAsync(Localisation.SessionsViewModel_DeleteSessionQuestion,
                style: MessageDialogStyle.AffirmativeAndNegative);
            if (MessageDialogResult.Affirmative == result)
            {
                var sessionInfo = SelectedSessionInfo;
                var exceptionMessage = String.Empty;
                if (null != sessionInfo)
                {
                    try
                    {
                        _sessionsService.Delete(sessionInfo.Id);
                        SessionInfos.Remove(sessionInfo);
                    }
                    catch (Exception ex)
                    {
                        exceptionMessage = ex.Message;
                    }
                }
                if (!String.IsNullOrEmpty(exceptionMessage))
                {
                    await MessageHelper.Instance.ShowMessageAsync(exceptionMessage);
                }
            }
        }

        private void ShowResults()
        {
            var handler = ShowResultsEvent;
            handler?.Invoke(this, null);
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
