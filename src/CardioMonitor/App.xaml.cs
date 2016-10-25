using CardioMonitor.DataBase;
using CardioMonitor.Devices;
using CardioMonitor.Devices.Monitor;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Files;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Infrastructure.Threading;
using CardioMonitor.Logs;
using CardioMonitor.Repositories;
using CardioMonitor.Repositories.Abstract;
using CardioMonitor.Settings;
using CardioMonitor.Ui.View;
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
            var container = Bootstrap();
            var mainWindow = container.GetInstance<MainWindow>();
            mainWindow.Show();
        }

        private static Container Bootstrap()
        {
            var container = new Container();

            container.RegisterSingleton(GetSettings);
            container.Register<ILogger, Logger>(Lifestyle.Singleton);
            container.Register<IDeviceControllerFactory, DeviceControllerFactory>(Lifestyle.Singleton);
            container.Register<TaskHelper, TaskHelper>(Lifestyle.Singleton);
            container.Register<DataBaseFactory, DataBaseFactory>(Lifestyle.Singleton);
            container.Register<IDataBaseRepository, DataBaseRepository>(Lifestyle.Singleton);
            container.Register<IPatientsRepository, PatientsRepository>(Lifestyle.Singleton);
            container.Register<ISessionsRepository, SessionsRepository>(Lifestyle.Singleton);
            container.Register<ITreatmentsRepository, TreatmentsRepository>(Lifestyle.Singleton);
            container.Register<IFilesManager, FilesManager>(Lifestyle.Singleton);

            container.Register<MainWindowViewModel, MainWindowViewModel>(Lifestyle.Transient);

            container.Verify();

            return container;
        }


        private static ICardioSettings GetSettings()
        {
            var settingsManager = new SettingsManager();
            return settingsManager.Load();
        }
    }
}
