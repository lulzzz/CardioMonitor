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
    public class SessionsViewModel : Notifier, IStoryboardPageViewModel
    {
        private readonly ISessionsService _sessionsService;
        private readonly IPatientsService _patientsService;
        private readonly ILogger _logger;
        private SessionWithPatientInfo _selectedSessionInfo;
        private ObservableCollection<SessionWithPatientInfo> _sessionInfos;

        private ICommand _startSessionCommand;
        private ICommand _deleteSessionCommand;
        private ICommand _showResultsCommand;

        public SessionsViewModel(
            ISessionsService sessionsService, 
            IPatientsService patientsService,
            [NotNull] ILogger logger)
        {
            _sessionsService = sessionsService ?? throw new ArgumentNullException(nameof(sessionsService));
            _patientsService = patientsService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        
        public SessionWithPatientInfo SelectedSessionInfo
        {
            get => _selectedSessionInfo;
            set
            {
                if (Equals(value, _selectedSessionInfo)) return;
                _selectedSessionInfo = value;
                RisePropertyChanged(nameof(SelectedSessionInfo));
                RisePropertyChanged(nameof(ShowResultsCommand));
                RisePropertyChanged(nameof(DeleteSessionCommand));
            }
        }

        public ObservableCollection<SessionWithPatientInfo> SessionInfos
        {
            get => _sessionInfos;
            set
            {
                if (Equals(value, _sessionInfos)) return;
                _sessionInfos = value;
                RisePropertyChanged(nameof(SessionInfos));
            }
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
            await PageTransitionRequested.InvokeAsync(
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
            var result = await MessageHelper.Instance.ShowMessageAsync(
                Localisation.SessionsViewModel_DeleteSessionQuestion,
                style: MessageDialogStyle.AffirmativeAndNegative);
            if (MessageDialogResult.Affirmative != result) return;

            var sessionInfo = SelectedSessionInfo;
            if (null == sessionInfo) return;


            try
            {
                IsBusy = true;
                BusyMessage = "Удаление сеанса...";
                await Task.Factory.StartNew(() => _sessionsService.Delete(sessionInfo.Id)).ConfigureAwait(true);
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
            Patient patient;
            try
            {
                IsBusy = true;
                BusyMessage = "Подготовка данных...";
                patient =
                    await Task.Factory.StartNew(() => _patientsService.GetPatient(SelectedSessionInfo.PatientId))
                        .ConfigureAwait(true);
            }
            catch (Exception e)
            {
                await MessageHelper.Instance.ShowMessageAsync(e.Message, "Cardio Monitor").ConfigureAwait(true);
                return;
            }
            finally
            {
                IsBusy = false;
            }

            await PageTransitionRequested.InvokeAsync(
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
            try
            {
                IsBusy = true;
                BusyMessage = "Загрузка сеансов...";
                var sessions = await Task.Factory.StartNew(() => _sessionsService.GetSessions())
                    .ConfigureAwait(true);
                SessionInfos = sessions != null
                    ? new ObservableCollection<SessionWithPatientInfo>(sessions)
                    : new ObservableCollection<SessionWithPatientInfo>();
                IsBusy = false;
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка загрузки сеансов. Причина: {ex.Message}",
                    ex);
                await MessageHelper.Instance.ShowMessageAsync("Ошибка загрузки сеансов");
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
