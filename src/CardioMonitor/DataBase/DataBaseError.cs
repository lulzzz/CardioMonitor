namespace CardioMonitor.DataBase
{
    /// <summary>
    /// Ошибки, которые могут возникнуть при работе с базой данных
    /// </summary>
    public enum DataBaseError
    {
        /// <summary>
        /// Неизвестная ошибка
        /// </summary>
        Unknown,
        /// <summary>
        /// Хост недоступен
        /// </summary>
        HostError, 
        /// <summary>
        /// Нет доступа
        /// </summary>
        /// <remarks>
        /// Возможно, некорректно заданы параметры подключения
        /// </remarks>
        AccessDenied
    }
}