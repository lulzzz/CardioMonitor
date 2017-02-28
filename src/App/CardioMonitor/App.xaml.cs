using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.SQLite.EF6;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.CoreContracts.Treatment;
using CardioMonitor.BLL.CoreServices.Patients;
using CardioMonitor.BLL.CoreServices.Sessions;
using CardioMonitor.BLL.CoreServices.Treatments;
using CardioMonitor.Data.Common.UnitOfWork;
using CardioMonitor.Data.Contracts.UnitOfWork;
using CardioMonitor.Data.Ef.Context;
using CardioMonitor.Data.Ef.UnitOfWork;
using CardioMonitor.Devices;
using CardioMonitor.Files;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Infrastructure.Threading;
using CardioMonitor.Logs;
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

            var settings = GetSettings();
            container.RegisterSingleton(settings);
            var context = new CardioMonitorContext();
            container.RegisterSingleton((DbContext) context);
            container.RegisterSingleton(context);
            container.Register<ILogger, Logger>(Lifestyle.Singleton);
            container.Register<IDeviceControllerFactory, DeviceControllerFactory>(Lifestyle.Singleton);
            container.Register<TaskHelper, TaskHelper>(Lifestyle.Singleton);
            container.Register<IUnitOfWorkContext, UnitOfWorkContext>(Lifestyle.Singleton);
            container.Register<ICardioMonitorUnitOfWorkFactory, CardioMonitorEfUnitOfWorkFactory>(Lifestyle.Singleton);
            container.Register<IPatientsService, PatientService>(Lifestyle.Singleton);
            container.Register<ITreatmentsService, TreatmentsService>(Lifestyle.Singleton);
            container.Register<ISessionsService, SessionsService>(Lifestyle.Singleton);
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
