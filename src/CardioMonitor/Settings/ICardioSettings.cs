namespace CardioMonitor.Settings
{
    public interface ICardioSettings
    {
        /// <summary>
        /// Полный путь к хранилищу файлов сессий
        /// </summary>
        string SessionsFilesDirectoryPath { get; set; }

        /// <summary>
        /// Настройки подключения к базе данных
        /// </summary>
        DataBaseSettings DataBase { get; set; }
    }
}