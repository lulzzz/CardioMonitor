using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CardioMonitor.Logs;

namespace CardioMonitor.Settings
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public class CardioSettings : ICardioSettings
    {
        
        #region Настройки внешнего вида

        //todo В текущей версии не используются
        /*
        public string SeletedAppThemeName { get; set; }
        public string SelectedAcentColorName { get; set; }
        */

        #endregion

        /// <summary>
        /// Полный путь к хранилищу файлов сессий
        /// </summary>
        public string SessionsFilesDirectoryPath { get; set; }

        /// <summary>
        /// Настройки подключения к базе данных
        /// </summary>
        public DataBaseSettings DataBase { get; set; }

        public CardioSettings()
        {
            try
            {
                //if (!Directory.Exists(SettingsPath))
                //{
                //    Directory.CreateDirectory(SettingsPath);
                //}
                SessionsFilesDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "CardioMonitor", "Patients");
                if (!Directory.Exists(SessionsFilesDirectoryPath))
                {
                    Directory.CreateDirectory(SessionsFilesDirectoryPath);
                }
                DataBase = new DataBaseSettings();
                DataBase.DataBase = "cardio_monitor_db";
                DataBase.Source = "localhost";
                DataBase.User = "root";

                DataBase.Password = "ropassot_123";
            }
            catch (Exception ex)
            {
                //Logger.Instance.LogError("CardioSettings", ex);
            }
        }
    }
}
