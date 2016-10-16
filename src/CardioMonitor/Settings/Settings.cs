using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CardioMonitor.Logs;

namespace CardioMonitor.Settings
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    [Serializable]
    public class Settings
    {
        /// <summary>
        /// Название приложения
        /// </summary>
        /// <remarks>
        /// Используется для создания директории для лог-файлов в AppData
        /// </remarks>
        public static readonly string AppName = "CardioMonitor";

        private static readonly string SettingsFileName = "Settings.bin";

        private static readonly string SettingsPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);


        private static volatile Settings _instance;
        private static readonly object SyncObject = new object();

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
        public string FilesDirectoryPath { get; set; }

        /// <summary>
        /// Настройки подключения к базе данных
        /// </summary>
        public DataBaseSettings DataBase { get; set; }

        private Settings()
        {
            try
            {
                if (!Directory.Exists(SettingsPath))
                {
                    Directory.CreateDirectory(SettingsPath);
                }
                FilesDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "CardioMonitor", "Patients");
                if (!Directory.Exists(FilesDirectoryPath))
                {
                    Directory.CreateDirectory(FilesDirectoryPath);
                }
                DataBase = new DataBaseSettings();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("Settings", ex);
            }
        }

        /// <summary>
        /// Настройки приложения
        /// </summary>
        public static Settings Instance
        {
            get
            {
                if (null != _instance)
                {
                    return _instance;
                }
                lock (SyncObject)
                {
                    if (null == _instance)
                    {
                        LoadFromFile();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Загрузить настройки из файла
        /// </summary>
        public static void LoadFromFile()
        {
            try
            {
                using (var loadingStream = new FileStream(Path.Combine(SettingsPath , SettingsFileName), FileMode.Open))
                {
                    var bf = new BinaryFormatter();
                    _instance = (Settings) bf.Deserialize(loadingStream);
                    loadingStream.Close();
                }
                //_instance.DataBase.DataBase = "cardio_monitor_db";
                //_instance.DataBase.Source = "localhost";
                //_instance.DataBase.User = "root";

                //_instance.DataBase.Password = "gfhjkm";
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("Settings", ex);
                _instance = new Settings();
                SaveToFile();
            }
        }

        /// <summary>
        /// Сохранить текущие настрйоки в файл
        /// </summary>
        public static void SaveToFile()
        {
            try
            {
                using (var savingStream = new FileStream(Path.Combine(SettingsPath, SettingsFileName), FileMode.Create))
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(savingStream, _instance);
                    savingStream.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("Settings", ex);
            }
        }
    }
}
