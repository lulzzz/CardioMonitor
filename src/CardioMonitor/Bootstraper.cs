using System;
using CardioMonitor.DataBase;
using CardioMonitor.Devices;
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
    public static class Bootstraper
    {
        [STAThread]
        public static void Main()
        {
            var container = Bootstrap();

            // Any additional other configuration, e.g. of your desired MVVM toolkit.

            RunApplication(container);
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

        private static void RunApplication(Container container)
        {
            try
            {
                var app = new App();
                var mainWindow = container.GetInstance<MainWindow>();
                app.Run(mainWindow);
            }
            catch (Exception ex)
            {
                //Log the exception and exit
            }
        }
    }
}