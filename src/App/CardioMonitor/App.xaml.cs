using System.IO;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.CoreServices.Patients;
using CardioMonitor.BLL.CoreServices.Sessions;
using CardioMonitor.BLL.SessionProcessing;
using CardioMonitor.Data.Ef.Context;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Bed.Fake.WpfModule;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Configuration;
using CardioMonitor.Devices.Data;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Devices.WpfModule;
using CardioMonitor.EventHandlers.Patients;
using CardioMonitor.Files;
using CardioMonitor.Infrastructure;
using CardioMonitor.Infrastructure.Workers;
using CardioMonitor.Settings;
using CardioMonitor.Ui;
using CardioMonitor.Ui.View;
using CardioMonitor.Ui.ViewModel;
using CardioMonitor.Ui.ViewModel.Patients;
using CardioMonitor.Ui.ViewModel.Sessions;
using CardioMonitor.Ui.ViewModel.Settings;
using Markeli.Storyboards;
using Markeli.Utils.EventBus.Contracts;
using Markeli.Utils.EventBus.Local;
using Markeli.Utils.Logging;
using Markeli.Utils.Logging.NLog;
using SimpleInjector;

namespace CardioMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static string NLogConfigName = "NLog.config";

        public App()
        {
            var container = Bootstrap();
            var mainWindowViewModel = container.GetInstance<MainWindowViewModel>();
            var mainWindow = new MainWindow(mainWindowViewModel);
            mainWindow.Show();
        }

        private static Container Bootstrap()
        {
            var container = new Container();

            RegisterInfrastructure(container);
            RegisterLogger(container);
            RegisterViewModels(container);
            RegisterDevices(container);
            RegisterServices(container);
            RegisterEventBus(container);
            RegisterPatientEventsHandlers(container);
            InitDevices(container);
            // throw exception, incorrect seleted lifycycles for disposable objects
            // container.Verify();

            return container;
        }

        private static void RegisterInfrastructure(Container container)
        {
            container.Register<IWorkerController, WorkerController>(Lifestyle.Singleton);
            container.Register<ICardioMonitorContextFactory>(() => new CardioMonitorContextFactory("CardioMonitorContext"), Lifestyle.Singleton);

            var settings = GetSettings();
            container.RegisterInstance(settings);

            container.Register<IStoryboardPageCreator, SimpleInjectorPageCreator>(Lifestyle.Transient);
            container.Register<StoryboardsNavigationService>(Lifestyle.Singleton);
            container.Register<IUiInvoker, WpfUiInvoker>(Lifestyle.Singleton);
        }

        private static ICardioSettings GetSettings()
        {
            var settingsManager = new SettingsManager();
            return settingsManager.Load();
        }

        private static void RegisterLogger(Container container)
        {
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), NLogConfigName);
            var logFactory = new NLoggerFactory(configPath);

            container.RegisterInstance<ILoggerFactory>(logFactory);
            container.Register(() => logFactory.CreateLogger(LogNames.MainLog));
        }

        private static void RegisterViewModels(Container container)
        {
            container.Register<MainWindowViewModel, MainWindowViewModel>(Lifestyle.Transient);
            container.Register<PatientViewModel>(Lifestyle.Transient);
            container.Register<PatientsViewModel>(Lifestyle.Transient);
            container.Register<PatientSessionsViewModel>(Lifestyle.Transient);
            container.Register<SessionDataViewModel>(Lifestyle.Transient);
            container.Register<SessionProcessingInitViewModel>(Lifestyle.Transient);
            container.Register<SessionProcessingViewModel>(Lifestyle.Transient);
            container.Register<SessionsViewModel>(Lifestyle.Transient);
            container.Register<SettingsViewModel>(Lifestyle.Transient);
        }

        private static void RegisterDevices(Container container)
        {
            container.Register<IDeviceControllerFactory, DeviceControllerFactory>(Lifestyle.Singleton);
            container.Register<IDeviceConfigurationService, DeviceConfigurationService>(Lifestyle.Singleton);
            container.Register<IDeviceModulesController, DeviceModulesController>(Lifestyle.Singleton);
            container.Register<IDeviceConfigurationContextFactory>(() => new DeviceConfigurationContextFactory("CardioMonitorContext"), Lifestyle.Singleton);
        }

        private static void RegisterServices(Container container)
        {
            container.Register<IPatientsService, PatientService>(Lifestyle.Transient);
            container.Register<ISessionsService, SessionsService>(Lifestyle.Transient);
            container.Register<IFilesManager, FilesManager>(Lifestyle.Transient);
            container.RegisterSingleton<ISessionParamsValidator, SessionParamsValidator>();
        }

        private static void RegisterEventBus(Container container)
        {
            container.RegisterSingleton<IEventBus, LocalEventBus>();
        }

        private static void RegisterPatientEventsHandlers(Container container)
        {
            container.Register<PatientAddedEventHandler>();
            container.Register<PatientChangedEventHandler>();
            container.Register<PatientDeletedEventHandler>();
        }

        private static void InitDevices(Container container)
        {
            var modulesController = container.GetInstance<IDeviceModulesController>();
            RegisterSupportedDevices(modulesController);
            modulesController.RegisterDevice(FakeDeviceControllerModule.Module);
        }

        private static void RegisterSupportedDevices(IDeviceModulesController controller)
        {
            controller.RegisterDeviceType(new DeviceTypeModule("Инверсионный стол", InversionTableDeviceTypeId.DeviceTypeId));
            controller.RegisterDeviceType(new DeviceTypeModule("Кардиомонитор", MonitorDeviceTypeId.DeviceTypeId));
        }
    }
}
