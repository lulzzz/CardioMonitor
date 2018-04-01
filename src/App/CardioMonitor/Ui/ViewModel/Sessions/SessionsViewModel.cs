using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Resources;
using CardioMonitor.Ui.Base;
using MahApps.Metro.Controls.Dialogs;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    public class SessionsViewModel : Notifier, IStoryboardPageViewModel
    {
        private readonly ISessionsService _sessionsService;
        private readonly IPatientsService _patientsService;
        private SessionWithPatientInfo _selectedSessionInfo;
        private ObservableCollection<SessionWithPatientInfo> _sessionInfos;

        private ICommand _startSessionCommand;
        private ICommand _deleteSessionCommand;
        private ICommand _showResultsCommand;

        public SessionsViewModel(
            ISessionsService sessionsService, IPatientsService patientsService)
        {
            _sessionsService = sessionsService ?? throw new ArgumentNullException(nameof(sessionsService));
            _patientsService = patientsService;
        }

        
        public SessionWithPatientInfo SelectedSessionInfo
        {
            get => _selectedSessionInfo;
            set
            {
                if (value != _selectedSessionInfo)
                {
                    _selectedSessionInfo = value;
                    RisePropertyChanged(nameof(SelectedSessionInfo));
                    RisePropertyChanged(nameof(ShowResultsCommand));
                    RisePropertyChanged(nameof(DeleteSessionCommand));
                }
            }
        }

        public ObservableCollection<SessionWithPatientInfo> SessionInfos
        {
            get => _sessionInfos;
            set
            {
                if (value != _sessionInfos)
                {
                    _sessionInfos = value;
                    RisePropertyChanged(nameof(SessionInfos));
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
                    ExecuteDelegate = async x => await StartSessionAsync()
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
                    ExecuteDelegate = async x => await DeleteSessionAsync()
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
                    ExecuteDelegate = async x => await ShowResultsAsync()
                });
            }
        }


        private async Task StartSessionAsync()
        {
            if (PageTransitionRequested == null) return;

            await PageTransitionRequested.Invoke(
                this,
                new TransitionRequest(
                    PageIds.SessionProcessingInitPageId,
                    new SessionProcessingInitPageContext
                    {
                        PatientId = _selectedSessionInfo?.PatientId
                    }))
                .ConfigureAwait(true);
        }

        private async Task DeleteSessionAsync()
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
                        await Task.Factory.StartNew(() => _sessionsService.Delete(sessionInfo.Id)).ConfigureAwait(true);
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

        private async Task ShowResultsAsync()
        {
            if (PageTransitionRequested == null) return;

            Patient patient;
            try
            {
                patient =
                    await Task.Factory.StartNew(() => _patientsService.GetPatient(SelectedSessionInfo.PatientId)).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                await MessageHelper.Instance.ShowMessageAsync(e.Message, "Cardio Monitor");
                return;
            }

            await PageTransitionRequested.Invoke(
                    this,
                    new TransitionRequest(
                        PageIds.SessionDataViewingPageId,
                        new SessionDataViewingPageContext
                        {
                            PatientId = patient.Id,
                            SessionId = SelectedSessionInfo.Id
                        }))
                .ConfigureAwait(true);
        }

        public void Clear()
        {
            SelectedSessionInfo = null;
            SessionInfos = new ObservableCollection<SessionWithPatientInfo>();
        }

        public void Dispose()
        {

        }

        private async Task LoadSessionsAsync()
        {
            var message = String.Empty;
            try
            {
                var sessions = await Task.Factory.StartNew(() => _sessionsService.GetSessions())
                    .ConfigureAwait(true);
                SessionInfos = sessions != null
                    ? new ObservableCollection<SessionWithPatientInfo>(sessions)
                    : new ObservableCollection<SessionWithPatientInfo>();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            if (!String.IsNullOrEmpty(message))
            {
                await MessageHelper.Instance.ShowMessageAsync(message);
            }

        }

        #region IStoryboardPageViewModel

        public Guid PageId { get; set; }
        public Guid StoryboardId { get; set; }

        public Task OpenAsync(IStoryboardPageContext context)
        {
            Task.Factory.StartNew(async () => await LoadSessionsAsync().ConfigureAwait(false));
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
            return Task.CompletedTask;
        }

        public Task<bool> CanCloseAsync()
        {
            return Task.FromResult(true);
        }

        public Task CloseAsync()
        {
            return Task.CompletedTask;
        }

        public event Func<object, Task> PageCanceled;
        public event Func<object, Task> PageCompleted;
        public event Func<object, Task> PageBackRequested;
        public event Func<object, TransitionRequest, Task> PageTransitionRequested;

        #endregion
    }
}
