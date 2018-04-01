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
    public class PatientSessionsViewModel : Notifier, IStoryboardPageViewModel
    {
        private readonly ISessionsService _sessionsService;
        private PatientFullName _patientName;
        private SessionInfo _selectedSessionInfo;
        private ObservableCollection<SessionInfo> _sessionInfos;

        private ICommand _startSessionCommand;
        private ICommand _deleteSessionCommand;
        private ICommand _showResultsCommand;

        public PatientSessionsViewModel(
            ISessionsService sessionsService)
        {
            _sessionsService = sessionsService ?? throw new ArgumentNullException(nameof(sessionsService));
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
                    ExecuteDelegate = async x =>await DeleteSessionAsync()
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

            //todo context
            await PageTransitionRequested.Invoke(
                this, 
                new TransitionRequest(
                    PageIds.SessionProcessingPageId, 
                    null))
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

            //todo context
            await PageTransitionRequested.Invoke(
                    this,
                    new TransitionRequest(
                        PageIds.SessionDataViewingPageId,
                        null))
                .ConfigureAwait(true);
        }

        public void Clear()
        {
            SelectedSessionInfo = null;
            SessionInfos = new ObservableCollection<SessionInfo>();
            PatientName = null;
        }

        public void Dispose()
        {

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
            throw new NotImplementedException();
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
