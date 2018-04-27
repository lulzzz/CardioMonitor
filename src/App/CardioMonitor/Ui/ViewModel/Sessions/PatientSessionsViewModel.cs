using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Resources;
using CardioMonitor.Ui.Base;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Markeli.Storyboards;
using Markeli.Utils.Logging;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    public class PatientSessionsViewModel : Notifier, IStoryboardPageViewModel
    {
        private readonly ISessionsService _sessionsService;
        private readonly ILogger _logger;
        private PatientFullName _patientName;
        private SessionInfo _selectedSessionInfo;
        private ObservableCollection<SessionInfo> _sessionInfos;
        private Patient _patient;

        private ICommand _startSessionCommand;
        private ICommand _deleteSessionCommand;
        private ICommand _showResultsCommand;

        public PatientSessionsViewModel(
            ISessionsService sessionsService,
            [NotNull] ILogger logger)
        {
            _sessionsService = sessionsService ?? throw new ArgumentNullException(nameof(sessionsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                RisePropertyChanged(nameof(IsBusy));
            }
        }
        private bool _isBusy;

        public string BusyMessage
        {
            get => _busyMessage;
            set
            {
                _busyMessage = value;
                RisePropertyChanged(nameof(BusyMessage));
            }
        }
        private string _busyMessage;

        public PatientFullName PatientName
        {
            get => _patientName;
            set
            {
                if (value == _patientName) return;
                _patientName = value;
                RisePropertyChanged(nameof(PatientName));
            }
        }

        public SessionInfo SelectedSessionInfo
        {
            get => _selectedSessionInfo;
            set
            {
                if (value == _selectedSessionInfo) return;
                _selectedSessionInfo = value;
                RisePropertyChanged(nameof(SelectedSessionInfo));
                RisePropertyChanged(nameof(ShowResultsCommand));
                RisePropertyChanged(nameof(DeleteSessionCommand));
            }
        }

        public ObservableCollection<SessionInfo> SessionInfos
        {
            get => _sessionInfos;
            set
            {
                if (value == _sessionInfos) return;
                _sessionInfos = value;
                RisePropertyChanged(nameof(SessionInfos));
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
            await PageTransitionRequested.InvokeAsync(
                this, 
                new TransitionRequest(
                    PageIds.SessionProcessingInitPageId, 
                    new SessionProcessingInitPageContext
                    {
                        PatientId = _patient.Id
                    }))
                .ConfigureAwait(true);
        }

        private async Task DeleteSessionAsync()
        {
            var result = await MessageHelper.Instance.ShowMessageAsync(
                Localisation.SessionsViewModel_DeleteSessionQuestion,
                style: MessageDialogStyle.AffirmativeAndNegative);
            if (MessageDialogResult.Affirmative != result) return;


            var sessionInfo = SelectedSessionInfo;
            if (null == sessionInfo) return;

            try
            {
                IsBusy = true;
                BusyMessage = "Удаление сеанса..";
                await _sessionsService.DeleteAsync(sessionInfo.Id).ConfigureAwait(true);
                SessionInfos.Remove(sessionInfo);
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка удаления сеанса с Id {sessionInfo.Id}. Причина: {ex.Message}",
                    ex);
                await MessageHelper.Instance.ShowMessageAsync("Ошибка удаления сеанса").ConfigureAwait(true);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ShowResultsAsync()
        {
            await PageTransitionRequested.InvokeAsync(
                    this,
                    new TransitionRequest(
                        PageIds.SessionDataViewingPageId,
                        new SessionDataViewingPageContext
                        {
                            PatientId = _patient.Id,
                            SessionId = SelectedSessionInfo.Id
                        }))
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

        private async Task LoadSessionsAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Загрузка сеансов...";
                var sessions = await  _sessionsService.GetPatientSessionInfosAsync(_patient.Id)
                    .ConfigureAwait(true);
                SessionInfos = sessions != null
                    ? new ObservableCollection<SessionInfo>(sessions)
                    : new ObservableCollection<SessionInfo>();
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка загрузки сеансов. Причина: {ex.Message}", ex);
                await MessageHelper.Instance.ShowMessageAsync("Ошибка загрузки сеансов").ConfigureAwait(true);
            }
            finally
            {
                IsBusy = false;
            }
        }

        #region IStoryboardPageViewModel
        
        public Guid PageId { get; set; }
        public Guid StoryboardId { get; set; }

        public Task OpenAsync(IStoryboardPageContext context)
        {
            if (!(context is PatientSessionsPageContext pageContext)) throw new ArgumentException("Incorrect type of arguments");

            _patient = pageContext.Patient;

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

        public event Func<TransitionEvent, Task> PageCanceled;
        public event Func<TransitionEvent, Task> PageCompleted;
        public event Func<TransitionEvent, Task> PageBackRequested;
        public event Func<object, TransitionRequest, Task> PageTransitionRequested;

        #endregion
    }
}
