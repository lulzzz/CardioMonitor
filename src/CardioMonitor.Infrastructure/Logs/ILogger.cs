using System;

namespace CardioMonitor.Infrastructure.Logs
{
    /// <summary>
    /// Интерфейс логгер, доступного из любого места приложения
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Добавляет запись об ошибке в лог
        /// </summary>
        /// <param name="className">Названия класса, в котором произошла ошибка</param>
        /// <param name="exceptionInfo">Текст записи</param>
        void LogError(string className, string exceptionInfo);

        /// <summary>
        /// Добавляет запись об ошибке в лог
        /// </summary>
        /// <param name="className">Название класса, в котором произошла ошибка</param>
        /// <param name="ex">Ошибка, информацию о которой следует сохранить</param>
        void LogError(string className, Exception ex);

        /// <summary>
        /// Добавляет в лог SQL запрос, который привел к ошибке 
        /// </summary>
        /// <param name="query">SQL запрос</param>
        void LogQueryError(string query);

        void Log(string message);
    }
}