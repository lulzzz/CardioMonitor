using CardioMonitor.Properties;

namespace CardioMonitor.Settings
{
    public interface ICardioSettings
    {
        /// <summary>
        /// Полный путь к хранилищу файлов сессий
        /// </summary>
        [NotNull]
        string SessionsFilesDirectoryPath { get; set; }

        /// <summary>
        /// Настройки подключения к базе данных
        /// </summary>
        [NotNull]
        DataBaseSettings DataBaseSettings { get; set; }
    }
}