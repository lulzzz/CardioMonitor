namespace CardioMonitor.Settings
{
    /// <summary>
    /// Настройки подключения к базе данных
    /// </summary>
    public class DataBaseSettings 
    {
        public DataBaseSettings()
        {
        }

        public DataBaseSettings(
            string dataBase, 
            string source, 
            string user, 
            string password)
        {
            DataBase = dataBase;
            Source = source;
            User = user;
            Password = password;
        }

        /// <summary>
        /// Название базы данных
        /// </summary>
        public string DataBase { get; private set; }
        /// <summary>
        /// Хост
        /// </summary>
        public string Source { get; private set; }
        /// <summary>
        /// Пользователь
        /// </summary>
        public string User { get; private set; }
        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; private set; }
    }
}
