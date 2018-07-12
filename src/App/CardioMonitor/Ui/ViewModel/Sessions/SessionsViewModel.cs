using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.EventHandlers.Sessions;
using CardioMonitor.Infrastructure.WpfCommon.Base;
using CardioMonitor.Resources;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Markeli.Storyboards;
using Markeli.Utils.Logging;
using ToastNotifications.Messages;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    public class SessionsViewModel : Notifier, IStoryboardPageViewModel
    {
        #region Fields

        private readonly ISessionsService _sessionsService;
        private readonly IPatientsService _patientsService;
        private readonly ILogger _logger;
        private SessionWithPatientInfo _selectedSessionInfo;
        private ObservableCollection<SessionWithPatientInfo> _sessionInfos;

        [NotNull]
        private readonly ToastNotifications.Notifier _notifier;

        private ICommand _startSessionCommand;
        private ICommand _deleteSessionCommand;
        private ICommand _showResultsCommand;
        private ICommand _loadSessionFromFileCommand;
        private string _busyMessage;

        private bool _isBusy;

        [NotNull]
        private readonly SessionAddedEventHandler _sessionAddedEventHandler;
        [NotNull]
        private readonly SessionChangedEventHandler _sessionChangedEventHandler;
        [NotNull]
        private readonly SessionDeletedEventHandler _sessionDeletedEventHandler;

        private bool _isSessionListChanged;

        [NotNull]
        private ISessionsFileUiManager _sessionFileManager;

        #endregion

        public SessionsViewModel(
            ISessionsService sessionsService, 
            IPatientsService patientsService,
            [NotNull] ILogger logger,
            [NotNull] ToastNotifications.Notifier notifier, 
            [NotNull] SessionAddedEventHandler sessionAddedEventHandler, 
            [NotNull] SessionChangedEventHandler sessionChangedEventHandler, 
            [NotNull] SessionDeletedEventHandler sessionDeletedEventHandler, 
            [NotNull] ISessionsFileUiManager sessionFileManager)
        {
            _sessionsService = sessionsService ?? throw new ArgumentNullException(nameof(sessionsService));
            _patientsService = patientsService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notifier = notifier;
            _sessionAddedEventHandler = sessionAddedEventHandler ?? throw new ArgumentNullException(nameof(sessionAddedEventHandler));
            _sessionChangedEventHandler = sessionChangedEventHandler ?? throw new ArgumentNullException(nameof(sessionChangedEventHandler));
            _sessionDeletedEventHandler = sessionDeletedEventHandler ?? throw new ArgumentNullException(nameof(sessionDeletedEventHandler));
            _sessionFileManager = sessionFileManager;

            _sessionAddedEventHandler.SessionAdded += delegate { _isSessionListChanged = true; };
            _sessionChangedEventHandler.SessionChanged += delegate { _isSessionListChanged = true; };
            _sessionDeletedEventHandler.SessionDeleted += delegate { _isSessionListChanged = true; };

            _isSessionListChanged = false;
        }

        #region Properties

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

        public string BusyMessage
        {
            get => _busyMessage;
            set
            {
                _busyMessage = value;
                RisePropertyChanged(nameof(BusyMessage));
            }
        }

        #endregion

        #region Commands

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

        public ICommand LoadSessionFromFileCommand
        {
            get
            {
                return _loadSessionFromFileCommand ?? (_loadSessionFromFileCommand = new SimpleCommand
                {
                    CanExecuteDelegate = x => true,
                    ExecuteDelegate = x => LoadSessionFromFileCommandExecute()
                });
            }
        }

        #endregion

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
                await  _sessionsService
                    .DeleteAsync(sessionInfo.Id)
                    .ConfigureAwait(true);
                SessionInfos.Remove(sessionInfo);
                _notifier.ShowError("Сеанс удален");
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка удаления сеанса с Id {sessionInfo.Id}. Причина: {ex.Message}",
                    ex);
                _notifier.ShowError("Ошибка удаления сеанса");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ShowResultsAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Подготовка данных...";
                var patient = await  _patientsService
                    .GetPatientAsync(SelectedSessionInfo.PatientId)
                    .ConfigureAwait(true);
                var session = await _sessionsService
                    .GetAsync(SelectedSessionInfo.Id)
                    .ConfigureAwait(true);

                await PageTransitionRequested.InvokeAsync(
                        this,
                        new TransitionRequest(
                            PageIds.SessionDataViewingPageId,
                            new SessionDataViewingPageContext
                            {
                                Patient = patient,
                                Session = session
                            }))
                    .ConfigureAwait(true);
            }
            catch (Exception e)
            {
                _notifier.ShowError("Ошибка подготовки данных для отображения результатов сеанса");
                _logger.Error($"{GetType().Name}: ошибка подготовка данных для отображения результатов сеанса. Причина: {e.Message}", e);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadSessionFromFileCommandExecute()
        {
            try
            {
                var sessionContainer = _sessionFileManager.Load();
                if (sessionContainer == null) return;
                await PageTransitionRequested.InvokeAsync(
                        this,
                        new TransitionRequest(
                            PageIds.SessionDataViewingPageId,
                            new SessionDataViewingPageContext
                            {
                                Patient = sessionContainer.Patient,
                                Session = sessionContainer.Session
                            }))
                    .ConfigureAwait(true);
            }
            catch (Exception e)
            {
                _notifier.ShowError("Ошибка просмотра сеанса из файла");
                _logger.Error(
                    $"{GetType().Name}: ошибка просмотра сеанса из файла. Причина: {e.Message}",
                    e);
            }
        }
        
        public void Clear()
        {
            SelectedSessionInfo = null;
            SessionInfos = new ObservableCollection<SessionWithPatientInfo>();
        }

        public void Dispose()
        {
            _sessionAddedEventHandler.Unsubscribe();
            _sessionDeletedEventHandler.Unsubscribe();
            _sessionChangedEventHandler.Unsubscribe();

            _sessionAddedEventHandler.Dispose();
            _sessionDeletedEventHandler.Dispose();
            _sessionChangedEventHandler.Dispose();
        }

        private async Task LoadSessionsSafeAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "Загрузка сеансов...";
                var sessions = await  _sessionsService
                    .GetAllAsync()
                    .ConfigureAwait(true);
                SessionInfos = sessions != null
                    ? new ObservableCollection<SessionWithPatientInfo>(sessions)
                    : new ObservableCollection<SessionWithPatientInfo>();
                IsBusy = false;
                _isSessionListChanged = false;
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка загрузки сеансов. Причина: {ex.Message}",
                    ex);
                _notifier.ShowError("Ошибка загрузки сеансов");
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
            _sessionAddedEventHandler.Subscribe();
            _sessionDeletedEventHandler.Subscribe();
            _sessionChangedEventHandler.Subscribe();

            Task.Factory.StartNew(async () => await LoadSessionsSafeAsync().ConfigureAwait(false));
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
            if (_isSessionListChanged)
            {
                Task.Factory.StartNew(async () => await LoadSessionsSafeAsync().ConfigureAwait(false));
            }

            return Task.CompletedTask;
        }

        public Task<bool> CanCloseAsync()
        {
            return Task.FromResult(true);
        }

        public Task CloseAsync()
        {
            _sessionAddedEventHandler.Unsubscribe();
            _sessionDeletedEventHandler.Unsubscribe();
            _sessionChangedEventHandler.Unsubscribe();

            return Task.CompletedTask;
        }

        public event Func<TransitionEvent, Task> PageCanceled;
        public event Func<TransitionEvent, Task> PageCompleted;
        public event Func<TransitionEvent, Task> PageBackRequested;
        public event Func<object, TransitionRequest, Task> PageTransitionRequested;

        #endregion
    }
}
