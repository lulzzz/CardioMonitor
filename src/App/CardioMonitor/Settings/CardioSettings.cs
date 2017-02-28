﻿namespace CardioMonitor.Settings
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
        /// Настройки подключения к базе данных (MySQL)
        /// </summary>
        public DataBaseSettings DataBaseSettings { get; set; }

        public string SqlLiteConnectionString { get; set; }
        
    }
}
