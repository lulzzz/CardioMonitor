using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace CardioMonitor.Settings
{
    public class SettingsManager
    {
        private readonly string ActiveConnectionStringName = "CardioMonitorContext";

        private readonly string SessionFilesDirectoryPathName = "SessionsFilesDirectoryPath";

        public void Save(ICardioSettings settings)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = configFile.AppSettings.Settings;
            var connectionString = settings.ConnectionString;
            //configFile.ConnectionStrings.ConnectionStrings[ActiveConnectionStringName].ConnectionString = connectionString;
            
            appSettings[SessionFilesDirectoryPathName].Value = settings.SessionsFilesDirectoryPath;

            configFile.Save(ConfigurationSaveMode.Modified, true);
            ConfigurationManager.RefreshSection("connectionStrings");
            ConfigurationManager.RefreshSection("appSettings");

        }

        //private string GetConnectionString(ICardioSettings settings)
        //{
        //    var builder = new SqlConnectionStringBuilder
        //    {
        //        ["Database"] = settings.DataBaseSettings.DataBase,
        //        ["Server"] = settings.DataBaseSettings.Source,
        //        ["User id"] = settings.DataBaseSettings.User,
        //        ["Password"] = settings.DataBaseSettings.Password,
        //        ["Port"] = settings.DataBaseSettings.Port
        //    };
        //    return builder.ConnectionString;
        //}

        public ICardioSettings Load()
        {

            var defaultSessionsFilesDirectoryPath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SettingsConstants.AppName);

            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            var sessionsFilesDirectoryPath = settings[SessionFilesDirectoryPathName]?.Value;
            if (String.IsNullOrEmpty(sessionsFilesDirectoryPath))
            {
                sessionsFilesDirectoryPath = defaultSessionsFilesDirectoryPath;
            }

            return new CardioSettings
            {
                SessionsFilesDirectoryPath = sessionsFilesDirectoryPath,
                ConnectionString = ConfigurationManager.ConnectionStrings[ActiveConnectionStringName].ConnectionString
            };
        }


        //private DataBaseSettings GetDataBaseSettings()
        //{
        //    var settings =
        //        ConfigurationManager.ConnectionStrings[ActiveConnectionStringName];
            
        //    if (settings == null)
        //        return null;

        //    return null;
        //    //return new DataBaseSettings(builder.InitialCatalog, builder.DataSource, builder.UserID, builder.Password);
        //}
    }
}