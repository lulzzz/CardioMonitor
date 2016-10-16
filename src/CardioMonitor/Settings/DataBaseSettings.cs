using System;

namespace CardioMonitor.Settings
{
    /// <summary>
    /// Настройки подключения к базе данных
    /// </summary>
    [Serializable]
    public class DataBaseSettings
    {
        /// <summary>
        /// Название базы данных
        /// </summary>
        public string DataBase { get; set; }
        /// <summary>
        /// Хост
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Пользователь
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; }
    }
}
