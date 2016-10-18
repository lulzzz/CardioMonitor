using CardioMonitor.DataBase;
using CardioMonitor.Devices;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Infrastructure.Threading;
using CardioMonitor.IoC;
using CardioMonitor.Logs;
using CardioMonitor.Repository;
using CardioMonitor.Settings;
using CardioMonitor.Ui.ViewModel;
using SimpleInjector;

namespace CardioMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            RegisterDependencies();
        }

        private void RegisterDependencies()
        {
            var container = new Container();
            IoCResolver.SetContainer(container);
            
            container.RegisterSingleton(GetSettings);
            container.Register<ILogger, Logger>(Lifestyle.Singleton);
            container.Register<IDeviceControllerFactory, DeviceControllerFactory>(Lifestyle.Singleton);
            container.Register<TaskHelper, TaskHelper>(Lifestyle.Singleton);
            container.Register<DataBaseFactory, DataBaseFactory>(Lifestyle.Singleton);
            container.Register<DataBaseRepository, DataBaseRepository>(Lifestyle.Singleton);
            container.Register<PatientsRepository, PatientsRepository>(Lifestyle.Singleton);
            container.Register<SessionsRepository, SessionsRepository>(Lifestyle.Singleton);
            container.Register<TreatmentsRepository, TreatmentsRepository>(Lifestyle.Singleton);
            container.Register<FileRepository, FileRepository>(Lifestyle.Singleton);

            container.Register<MainWindowViewModel, MainWindowViewModel>(Lifestyle.Transient);

            container.Verify();
            
        }

        private ICardioSettings GetSettings()
        {
            var settingsManager = new SettingsManager();
            return settingsManager.Load();
        }

    }
}
