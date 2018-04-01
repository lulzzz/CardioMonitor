using System.Windows;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.CoreServices.Patients;
using CardioMonitor.BLL.CoreServices.Sessions;
using CardioMonitor.Data.Contracts.UnitOfWork;
using CardioMonitor.Data.Ef.UnitOfWork;
using CardioMonitor.Devices;
using CardioMonitor.Files;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Infrastructure.Threading;
using CardioMonitor.Infrastructure.Workers;
using CardioMonitor.Logs;
using CardioMonitor.Settings;
using CardioMonitor.Ui;
using CardioMonitor.Ui.View;
using CardioMonitor.Ui.ViewModel;
using CardioMonitor.Ui.ViewModel.Patients;
using CardioMonitor.Ui.ViewModel.Sessions;
using CardioMonitor.Ui.ViewModel.Settings;
using Markeli.Storyboards;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace CardioMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            //todo add startup window
            var container = Bootstrap();
            var mainWindow = container.GetInstance<MainWindow>();
            mainWindow.Show();
        }

        private static Container Bootstrap()
        {
            var container = new Container();

            var settings = GetSettings();
            container.RegisterSingleton(settings);
            container.Register<ILogger, Logger>(Lifestyle.Singleton);
            container.Register<IDeviceControllerFactory, DeviceControllerFactory>(Lifestyle.Singleton);
            container.Register<TaskHelper>(Lifestyle.Singleton);
            container.Register<ICardioMonitorUnitOfWorkFactory>(() => new CardioMonitorEfUnitOfWorkFactory("CardioMonitorContext"), Lifestyle.Singleton);
            container.Register<IPatientsService, PatientService>(Lifestyle.Singleton);
            container.Register<ISessionsService, SessionsService>(Lifestyle.Singleton);
            container.Register<IFilesManager, FilesManager>(Lifestyle.Singleton);
            
            container.Register<IWorkerController, WorkerController>(Lifestyle.Singleton);

            container.Register<MainWindowViewModel, MainWindowViewModel>(Lifestyle.Transient);
            container.Register<PatientViewModel>(Lifestyle.Transient);
            container.Register<PatientsViewModel>(Lifestyle.Transient);
            container.Register<PatientSessionsViewModel>(Lifestyle.Transient);
            container.Register<SessionDataViewModel>(Lifestyle.Transient);
            container.Register<SessionProcessingInitViewModel>(Lifestyle.Transient);
            container.Register<SessionProcessingViewModel>(Lifestyle.Transient);
            container.Register<SessionsViewModel>(Lifestyle.Transient);
            container.Register<SettingsViewModel>(Lifestyle.Transient);
            container.Register<IStoryboardPageCreator, SimpleInjectorPageCreator>(Lifestyle.Transient);
            container.Register<StoryboardsNavigationService>(Lifestyle.Singleton);
            container.Register<IUiInvoker, WpfUiInvoker>(Lifestyle.Singleton);

            // container.Verify();
            return container;
        }


        private static ICardioSettings GetSettings()
        {
            var settingsManager = new SettingsManager();
            return settingsManager.Load();
        }
    }
}
