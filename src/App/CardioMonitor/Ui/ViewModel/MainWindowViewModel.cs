using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Devices;
using CardioMonitor.Files;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Infrastructure.Threading;
using CardioMonitor.Infrastructure.Workers;
using CardioMonitor.Resources;
using CardioMonitor.Settings;
using CardioMonitor.Ui.Base;
using CardioMonitor.Ui.View.Patients;
using CardioMonitor.Ui.View.Sessions;
using CardioMonitor.Ui.View.Settings;
using CardioMonitor.Ui.ViewModel.Patients;
using CardioMonitor.Ui.ViewModel.Sessions;
using CardioMonitor.Ui.ViewModel.Settings;
using JetBrains.Annotations;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.ViewModel{

    internal enum ViewIndex
    {
        NotSelected = -1,
        PatientsView = 0,
        TreatmentsView = 1,
        SessionsView = 2,
        SessionView = 3,
        TreatmentDataView = 4,
        SessionDataView = 5,
        PatientView = 6
    }

    public class MainWindowViewModel : Notifier
    {
        private readonly ILogger _logger;
        private readonly IPatientsService _patientsService;
        private readonly ISessionsService _sessionsService;
        private readonly IFilesManager _filesRepository;
        private ICommand _moveBackwardComand;

        private readonly StoryboardsNavigationService _storyboardsNavigationService;

        #region Свойства

        public ICommand MoveBackwardCommand
        {
            get
            {
                return _moveBackwardComand ??
                       (_moveBackwardComand =
                           new SimpleCommand
                           {
                               CanExecuteDelegate =  x =>  _storyboardsNavigationService.CanGoBack(),
                               ExecuteDelegate = async x => await _storyboardsNavigationService.GoBackAsync().ConfigureAwait(true)
                           });
            }
        }


        public Storyboard PatientsStoryboard
        {
            get => _patientsStoryboard;
            set
            {
                if (_patientsStoryboard != value)
                {
                    if (_patientsStoryboard != null)
                    {
                        _patientsStoryboard.PropertyChanged -= PatientsStoryboardOnPropertyChanged;
                    }
                    _patientsStoryboard = value;
                    if (_patientsStoryboard != null)
                    {

                        _patientsStoryboard.PropertyChanged += PatientsStoryboardOnPropertyChanged;
                    }
                }
            }
        }

        private void PatientsStoryboardOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            PatientsView = _patientsStoryboard.ActivePage as UIElement;
        }

        private Storyboard _patientsStoryboard;


        public UIElement PatientsView
        {
            get => _patientsView;
            set
            {
                if (!Equals(_patientsView, value))
                {
                    _patientsView = value;
                    RisePropertyChanged(nameof(PatientsView));
                }
            }
        }
        private UIElement _patientsView;

        public Storyboard SessionsStoryboard
        {
            get => _sessionStoryboard;
            set
            {
                if (_sessionStoryboard != value)
                {
                    _sessionStoryboard = value;
                    RisePropertyChanged(nameof(SessionsStoryboard));
                }
            }
        }
        private Storyboard _sessionStoryboard;

        public Storyboard SessionProcessingStoryboard
        {
            get => _sessionProcessingStoryboard;
            set
            {
                if (_sessionProcessingStoryboard != value)
                {
                    _sessionProcessingStoryboard = value;
                    RisePropertyChanged(nameof(SessionProcessingStoryboard));
                }
            }
        }
        private Storyboard _sessionProcessingStoryboard;

        public Storyboard SettingsStoryboard
        {
            get => _settingsStoryboard;
            set
            {
                if (_settingsStoryboard != value)
                {
                    _settingsStoryboard = value;
                    RisePropertyChanged(nameof(SettingsStoryboard));
                }
            }
        }
        private Storyboard _settingsStoryboard;

        #endregion

        public MainWindowViewModel(
            ILogger logger,
            IPatientsService patientsService,
            ISessionsService sessionsService,
            IFilesManager filesRepository,
            IDeviceControllerFactory deviceControllerFactory,
            ICardioSettings settings,
            TaskHelper taskHelper,
            [NotNull] IWorkerController workerController, 
            [NotNull] StoryboardsNavigationService storyboardsNavigationService,
            IStoryboardPageCreator pageCreator,
            IUiInvoker invoker)
        {
            if (deviceControllerFactory == null) throw new ArgumentNullException(nameof(deviceControllerFactory));
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (taskHelper == null) throw new ArgumentNullException(nameof(taskHelper));
            if (workerController == null) throw new ArgumentNullException(nameof(workerController));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _patientsService = patientsService ?? throw new ArgumentNullException(nameof(patientsService));
            _sessionsService = sessionsService ?? throw new ArgumentNullException(nameof(sessionsService));
            _filesRepository = filesRepository ?? throw new ArgumentNullException(nameof(filesRepository));

            _storyboardsNavigationService = storyboardsNavigationService ?? throw new ArgumentNullException(nameof(storyboardsNavigationService));
            _storyboardsNavigationService.CanBackChanged += RiseCanGoBackChaned;
            _storyboardsNavigationService.SetUiInvoker(invoker);

            PatientsStoryboard = new Storyboard(StoryboardIds.PatientsStoryboardId);

            PatientsStoryboard.RegisterPage(
                PageIds.PatientsPageId,
                view: typeof(PatientsView),
                viewModel: typeof(PatientsViewModel),
                isStartPage: true);

            PatientsStoryboard.RegisterPage(
                PageIds.PatientPageId,
                view: typeof(PatientView),
                viewModel: typeof(PatientViewModel),
                isStartPage: false);

            PatientsStoryboard.RegisterPage(
                PageIds.PatientSessionsPageId,
                view: typeof(PatientSessionsView),
                viewModel: typeof(PatientSessionsViewModel),
                isStartPage: false);

            PatientsStoryboard.RegisterPage(
                PageIds.SessionDataViewingPageId,
                view: typeof(SessionDataView),
                viewModel: typeof(SessionDataViewModel),
                isStartPage: false);


            SessionsStoryboard = new Storyboard(StoryboardIds.SessionsStoryboardId);
            
            SessionsStoryboard.RegisterPage(
                PageIds.SessionsPageId,
                view: typeof(SessionsView),
                viewModel: typeof(SessionsViewModel),
                isStartPage: true);

            SessionsStoryboard.RegisterPage(
                PageIds.SessionDataViewingPageId,
                view: typeof(SessionDataView),
                viewModel: typeof(SessionDataViewModel),
                isStartPage: false);
            
            SessionProcessingStoryboard = new Storyboard(StoryboardIds.SessionProcessingStoryboardId);

            SessionProcessingStoryboard.RegisterPage(
                PageIds.SessionProcessingInitPageId,
                view: typeof(SessionProcessingInitView),
                viewModel: typeof(SessionProcessingInitViewModel),
                isStartPage: true);

            SessionProcessingStoryboard.RegisterPage(
                PageIds.SessionProcessingPageId,
                view: typeof(SessionProcessingView),
                viewModel: typeof(SessionProcessingViewModel),
                isStartPage: false);

            SettingsStoryboard = new Storyboard(StoryboardIds.SettingsStoryboardId);

            SettingsStoryboard.RegisterPage(
                PageIds.SettingsPageId,
                view: typeof(SettingsView),
                viewModel: typeof(SettingsViewModel),
                isStartPage: true);

            _storyboardsNavigationService.RegisterStoryboard(PatientsStoryboard);
            _storyboardsNavigationService.RegisterStoryboard(SessionsStoryboard);
            _storyboardsNavigationService.RegisterStoryboard(SessionProcessingStoryboard);
            _storyboardsNavigationService.RegisterStoryboard(SettingsStoryboard);

            _storyboardsNavigationService.SetStoryboardPageCreator(pageCreator);

            var startPageContexts = new Dictionary<Guid, IStoryboardPageContext>
            {
                [PageIds.SessionProcessingInitPageId] = new SessionProcessingInitPageContext()
            };

            _storyboardsNavigationService.CreateStartPages(startPageContexts);
        }

        private void RiseCanGoBackChaned(object sender, EventArgs args)
        {
            RisePropertyChanged(nameof(MoveBackwardCommand));
        }

        public void OpenStartStoryboard()
        {
            Task.Factory.StartNew(async () =>
                {
                    try
                    {

                        await _storyboardsNavigationService.GoToStoryboardAsync(StoryboardIds.PatientsStoryboardId)
                            .ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        _logger.Log(e.Message);
                    }
                });
        }
        

        private async void LoadSession(object sender, EventArgs args)
        {
            /*
            var loadDialog = new OpenFileDialog {CheckFileExists = true, Filter = Localisation.FileRepository_SeansFileFilter};
            var result = loadDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var message = String.Empty;
                try
                {
                    var container = _filesRepository.LoadFromFile(loadDialog.FileName);
                }
                catch (ArgumentNullException)
                {
                    message = Localisation.ArgumentNullExceptionMessage;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
                if (!String.IsNullOrEmpty(message))
                {
                    await MessageHelper.Instance.ShowMessageAsync(message);
                }
            }*/
        }
    }

  
}
