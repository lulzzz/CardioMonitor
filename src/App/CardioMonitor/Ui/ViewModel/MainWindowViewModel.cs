using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CardioMonitor.Infrastructure.WpfCommon.Base;
using CardioMonitor.Ui.View.Devices;
using CardioMonitor.Ui.View.Patients;
using CardioMonitor.Ui.View.Sessions;
using CardioMonitor.Ui.View.Settings;
using CardioMonitor.Ui.ViewModel.Devices;
using CardioMonitor.Ui.ViewModel.Patients;
using CardioMonitor.Ui.ViewModel.Sessions;
using CardioMonitor.Ui.ViewModel.Settings;
using JetBrains.Annotations;
using MahApps.Metro.IconPacks;
using Markeli.Storyboards;
using Markeli.Utils.Logging;
using ToastNotifications.Messages;

namespace CardioMonitor.Ui.ViewModel{


    public class MainWindowViewModel : Notifier
    {
        private readonly StoryboardsNavigationService _storyboardsNavigationService;
        private readonly ILogger _logger;
        [NotNull] 
        private readonly ToastNotifications.Notifier _notifier;

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
        private ICommand _moveBackwardComand;
        

        public bool IsIconVisible
        {
            get => _isIconVisible;
            set
            {
                _isIconVisible = value;
                RisePropertyChanged(nameof(IsIconVisible));
            }
        }
        private bool _isIconVisible;

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

        public ExtendedStoryboard CurrentOpennedStoryboard
        {
            get => _currentOpennedStoryboard;
            set
            {
                if (Equals(_currentOpennedStoryboard, value)) return;
                _currentOpennedStoryboard = value;
                RisePropertyChanged(nameof(CurrentOpennedStoryboard));
            }
        }
        private ExtendedStoryboard _currentOpennedStoryboard;

        #endregion

        public MainWindowViewModel( 
            [NotNull] StoryboardsNavigationService storyboardsNavigationService,
            IStoryboardPageCreator pageCreator,
            IUiInvoker invoker,
            [NotNull] ILogger logger,
            [NotNull] ToastNotifications.Notifier notifier)
        {
            if (notifier == null) throw new ArgumentNullException(nameof(notifier));
            _storyboardsNavigationService = storyboardsNavigationService ?? throw new ArgumentNullException(nameof(storyboardsNavigationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notifier = notifier;
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

            patientsStoryboard.RegisterTransition(PageIds.PatientPageId, PageIds.PatientsPageId, PageTransitionTrigger.Completed);
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

            var deviceConfigStoryboard = new ExtendedStoryboard(
                StoryboardIds.DevicesStoryboardId,
                "Устройства",
                new PackIconOcticons { Kind = PackIconOcticonsKind.Rocket });

            deviceConfigStoryboard.RegisterPage(
                PageIds.DevicesPageId,
                view: typeof(DeviceConfigsView),
                viewModel: typeof(DeviceConfigsViewModel),
                isStartPage: true);

            deviceConfigStoryboard.RegisterPage(
                PageIds.DeviceCreationPageId,
                view: typeof(DeviceConfigCreationView),
                viewModel: typeof(DeviceConfigCreationViewModel),
                isStartPage: false);

            deviceConfigStoryboard.RegisterTransition(PageIds.DeviceCreationPageId, PageIds.DevicesPageId, PageTransitionTrigger.Completed);

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
            _storyboardsNavigationService.RegisterStoryboard(deviceConfigStoryboard);
            _storyboardsNavigationService.RegisterStoryboard(settingsStoryboard);

            _storyboardsNavigationService.SetStoryboardPageCreator(pageCreator);

            var startPageContexts = new Dictionary<Guid, IStoryboardPageContext>
            {
                [PageIds.SessionProcessingInitPageId] = new SessionProcessingInitPageContext()
            };

            _storyboardsNavigationService.CreateStartPages(startPageContexts);

            _storyboardsNavigationService.ActiveStoryboardChanged += HandeActiveStoryboardChanged;
            _storyboardsNavigationService.ExceptionOccured += HandleNavigationError;

            ItemStoryboards = new ObservableCollection<ExtendedStoryboard>(new []
            {
                patientsStoryboard,
                sessionsStoryboard,
                sessionProcessingStoryboard,
                deviceConfigStoryboard
            });
            
            OptionsStoryboards = new ObservableCollection<ExtendedStoryboard>(new []
            {
                settingsStoryboard
            });
        }

        private void HandeActiveStoryboardChanged(object sender, Guid guid)
        {
            var storyboard =
                ItemStoryboards.FirstOrDefault(x => x.StoryboardId == guid);
            if (storyboard != null)
            {
                SelectedItemStoryboard = storyboard;
                SelectedOptionsStoryboard = null;
                CurrentOpennedStoryboard = storyboard;
                return;
            }

            storyboard =
                OptionsStoryboards.FirstOrDefault(x => x.StoryboardId == guid);
            SelectedOptionsStoryboard = storyboard;
            SelectedItemStoryboard = null;
            CurrentOpennedStoryboard = storyboard;
        }

        private void HandleNavigationError(object sender, Exception ex)
        {
            _notifier.ShowError("Произошла ошибка при переходе на другую страницу");
            _logger.Error($"{GetType().Name}: Ошибка перехода на другую страницу. Причина: {ex.Message}", ex);
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
                        // called twice, fix it
                        await _storyboardsNavigationService.GoToStoryboardAsync(StoryboardIds.PatientsStoryboardId)
                            .ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"{GetType().Name}: ошибка перехода на storyboard с Id {StoryboardIds.PatientsStoryboardId}. Причина: {e.Message} ", e);
                    }
                });
        }
    }
}
