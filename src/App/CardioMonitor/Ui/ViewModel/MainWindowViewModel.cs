using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
using CardioMonitor.Ui.View;
using CardioMonitor.Ui.View.Patients;
using CardioMonitor.Ui.View.Sessions;
using CardioMonitor.Ui.View.Settings;
using CardioMonitor.Ui.ViewModel.Patients;
using CardioMonitor.Ui.ViewModel.Sessions;
using CardioMonitor.Ui.ViewModel.Settings;
using JetBrains.Annotations;
using MahApps.Metro.IconPacks;
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
        
        private readonly ICollection<ExtendedStoryboard> _storyboards;
        

        #endregion

        private bool _isIconVisible;

        public bool IsIconVisible
        {
            get => _isIconVisible;
            set
            {
                _isIconVisible = value;
                RisePropertyChanged(nameof(IsIconVisible));
            }
        }

        public ObservableCollection<ExtendedStoryboard> ItemStoryboards
        {
            get => _itemStoryboards;
            set
            {
                if (Equals(_itemStoryboards, value)) return;
                _itemStoryboards = value;
                RisePropertyChanged(nameof(ItemStoryboards));
            }
        }
        private ObservableCollection<ExtendedStoryboard> _itemStoryboards;

        public ObservableCollection<ExtendedStoryboard> OptionsStoryboards
        {
            get => _optionsStoryboards;
            set
            {
                if (Equals(_optionsStoryboards, value)) return;
                _optionsStoryboards = value; 
                RisePropertyChanged(nameof(OptionsStoryboards));
            }
        }
        private ObservableCollection<ExtendedStoryboard> _optionsStoryboards;


        public ExtendedStoryboard SelectedItemStoryboard
        {
            get => _selectedItemStoryboard;
            set
            {
                if (Equals(_selectedItemStoryboard, value)) return;
                _selectedItemStoryboard = value;
                RisePropertyChanged(nameof(SelectedItemStoryboard));
                // monkey hack
                if (SelectedItemStoryboard != null)
                {
                    _storyboardsNavigationService.GoToStoryboardAsync(_selectedItemStoryboard.StoryboardId);
                }
            }
        }
        private ExtendedStoryboard _selectedItemStoryboard;

        public ExtendedStoryboard SelectedOptionsStoryboard
        {
            get => _selectedOptionsStoryboard;
            set
            {
                if (Equals(_selectedOptionsStoryboard, value)) return;
                _selectedOptionsStoryboard = value;
                RisePropertyChanged(nameof(SelectedOptionsStoryboard));
                if (SelectedOptionsStoryboard != null)
                {
                    _storyboardsNavigationService.GoToStoryboardAsync(_selectedOptionsStoryboard.StoryboardId);
                }
               
            }
        }
        private ExtendedStoryboard _selectedOptionsStoryboard;

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

            var patientsStoryboard = new ExtendedStoryboard(
                StoryboardIds.PatientsStoryboardId,
                "Пациенты",
                new PackIconMaterial{Kind = PackIconMaterialKind.Account});

            patientsStoryboard.RegisterPage(
                PageIds.PatientsPageId,
                view: typeof(PatientsView),
                viewModel: typeof(PatientsViewModel),
                isStartPage: true);

            patientsStoryboard.RegisterPage(
                PageIds.PatientPageId,
                view: typeof(PatientView),
                viewModel: typeof(PatientViewModel),
                isStartPage: false);

            patientsStoryboard.RegisterPage(
                PageIds.PatientSessionsPageId,
                view: typeof(PatientSessionsView),
                viewModel: typeof(PatientSessionsViewModel),
                isStartPage: false);

            patientsStoryboard.RegisterPage(
                PageIds.SessionDataViewingPageId,
                view: typeof(SessionDataView),
                viewModel: typeof(SessionDataViewModel),
                isStartPage: false);

            patientsStoryboard.RegisterTransition(PageIds.PatientPageId, PageIds.PatientsPageId, PageTransitionTrigger.Back);
            patientsStoryboard.RegisterTransition(PageIds.PatientSessionsPageId, PageIds.PatientsPageId, PageTransitionTrigger.Back);


            var sessionsStoryboard = new ExtendedStoryboard(
                StoryboardIds.SessionsStoryboardId,
                "Все сеансы",
                new PackIconOcticons{Kind = PackIconOcticonsKind.Checklist });

            sessionsStoryboard.RegisterPage(
                PageIds.SessionsPageId,
                view: typeof(SessionsView),
                viewModel: typeof(SessionsViewModel),
                isStartPage: true);

            sessionsStoryboard.RegisterPage(
                PageIds.SessionDataViewingPageId,
                view: typeof(SessionDataView),
                viewModel: typeof(SessionDataViewModel),
                isStartPage: false);

            sessionsStoryboard.RegisterTransition(PageIds.SessionDataViewingPageId, PageIds.SessionsPageId, PageTransitionTrigger.Back);

            var sessionProcessingStoryboard = new ExtendedStoryboard(
                StoryboardIds.SessionProcessingStoryboardId,
                "Новый сеанс",
                new PackIconOcticons { Kind = PackIconOcticonsKind.Pulse});

            sessionProcessingStoryboard.RegisterPage(
                PageIds.SessionProcessingInitPageId,
                view: typeof(SessionProcessingInitView),
                viewModel: typeof(SessionProcessingInitViewModel),
                isStartPage: true);

            sessionProcessingStoryboard.RegisterPage(
                PageIds.SessionProcessingPageId,
                view: typeof(SessionProcessingView),
                viewModel: typeof(SessionProcessingViewModel),
                isStartPage: false);

            sessionProcessingStoryboard.RegisterTransition(PageIds.SessionProcessingPageId, PageIds.SessionProcessingInitPageId, PageTransitionTrigger.Back);

            var settingsStoryboard = new ExtendedStoryboard(
                StoryboardIds.SettingsStoryboardId,
                "Настройки",
                new PackIconMaterial{ Kind = PackIconMaterialKind.Settings});

            settingsStoryboard.RegisterPage(
                PageIds.SettingsPageId,
                view: typeof(SettingsView),
                viewModel: typeof(SettingsViewModel),
                isStartPage: true);

            _storyboardsNavigationService.RegisterStoryboard(patientsStoryboard);
            _storyboardsNavigationService.RegisterStoryboard(sessionsStoryboard);
            _storyboardsNavigationService.RegisterStoryboard(sessionProcessingStoryboard);
            _storyboardsNavigationService.RegisterStoryboard(settingsStoryboard);

            _storyboardsNavigationService.SetStoryboardPageCreator(pageCreator);

            var startPageContexts = new Dictionary<Guid, IStoryboardPageContext>
            {
                [PageIds.SessionProcessingInitPageId] = new SessionProcessingInitPageContext()
            };

            _storyboardsNavigationService.CreateStartPages(startPageContexts);

            _storyboardsNavigationService.ActiveStoryboardChanged += StoryboardsNavigationServiceOnActiveStoryboardChanged;

            _storyboards = new List<ExtendedStoryboard>
            {
                patientsStoryboard,
                sessionsStoryboard,
                sessionProcessingStoryboard,
                sessionProcessingStoryboard
            };

            ItemStoryboards = new ObservableCollection<ExtendedStoryboard>(new []
            {
                patientsStoryboard,
                sessionsStoryboard,
                sessionProcessingStoryboard
            });

            OptionsStoryboards = new ObservableCollection<ExtendedStoryboard>(new []
            {
                settingsStoryboard
            });
        }

        private void StoryboardsNavigationServiceOnActiveStoryboardChanged(object sender, Guid guid)
        {
            var storyboard =
                ItemStoryboards.FirstOrDefault(x => x.StoryboardId == guid);
            if (storyboard != null)
            {
                SelectedItemStoryboard = storyboard;
                return;
            }

            storyboard =
                OptionsStoryboards.FirstOrDefault(x => x.StoryboardId == guid);
            SelectedOptionsStoryboard = storyboard;
        }

        private void RiseCanGoBackChaned(object sender, EventArgs args)
        {
            RisePropertyChanged(nameof(MoveBackwardCommand));
            IsIconVisible = !MoveBackwardCommand?.CanExecute(null) ?? true;
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
