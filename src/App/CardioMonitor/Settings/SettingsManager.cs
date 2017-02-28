using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace CardioMonitor.Settings
{
    public class SettingsManager
    {
        private readonly string ActiveConnectionStringName = "Active";
        private readonly string SqlLiteConnectionStringName = "SqlLite";

        private readonly string SessionFilesDirectoryPathName = "SessionsFilesDirectoryPath";

        public void Save(ICardioSettings settings)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = configFile.AppSettings.Settings;
            var connectionString = GetConnectionString(settings);
            configFile.ConnectionStrings.ConnectionStrings[ActiveConnectionStringName].ConnectionString = connectionString;

            configFile.ConnectionStrings.ConnectionStrings[SqlLiteConnectionStringName].ConnectionString 
                = settings.SqlLiteConnectionString;
            appSettings[SessionFilesDirectoryPathName].Value = settings.SessionsFilesDirectoryPath;

            configFile.Save(ConfigurationSaveMode.Modified, true);
            ConfigurationManager.RefreshSection("connectionStrings");
            ConfigurationManager.RefreshSection("appSettings");

        }

        private string GetConnectionString(ICardioSettings settings)
        {
            var  builder = new SqlConnectionStringBuilder();
            builder["DATABASE"] = settings.DataBaseSettings.DataBase;
            builder["SERVER"] = settings.DataBaseSettings.Source;
            builder["UID"] = settings.DataBaseSettings.User;
            builder["PASSWORD"] = settings.DataBaseSettings.Password;
            return builder.ConnectionString;
        }

        public ICardioSettings Load()
        {
            var dataBaseSettings = GetDataBaseSettings();

            var defaultSessionsFilesDirectoryPath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SettingsConstants.AppName);

            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            var sessionsFilesDirectoryPath = settings[SessionFilesDirectoryPathName]?.Value;
            if (String.IsNullOrEmpty(sessionsFilesDirectoryPath))
            {
                sessionsFilesDirectoryPath = defaultSessionsFilesDirectoryPath;
            }
            //var sqlLiteSettings =
            //    ConfigurationManager.ConnectionStrings[SqlLiteConnectionStringName].ConnectionString;

            return new CardioSettings
            {
                DataBaseSettings = dataBaseSettings,
                SessionsFilesDirectoryPath = sessionsFilesDirectoryPath,
                //SqlLiteConnectionString = sqlLiteSettings
            };
        }


        private DataBaseSettings GetDataBaseSettings()
        {
            var settings =
                ConfigurationManager.ConnectionStrings[ActiveConnectionStringName];
            
            if (settings == null)
                return null;

            var builder = new SqlConnectionStringBuilder(settings.ConnectionString);
            return new DataBaseSettings(builder.InitialCatalog, builder.DataSource, builder.UserID, builder.Password);
        }
    }
}