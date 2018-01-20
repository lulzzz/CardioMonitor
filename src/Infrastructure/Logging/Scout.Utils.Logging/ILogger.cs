using System;

namespace Scout.Utils.Logging
{
    /// <summary>
    /// Логгер
    /// </summary>
    /// <remarks>
    /// Выступает в качестве фасада для внешних логирующих систем
    /// </remarks>
    public interface ILogger
    {
        /// <summary>
        /// Имя логгера
        /// </summary>
        string Name { get; }

        #region Debug
        
        /// <summary>
        /// Записывает в лог уровня Debug
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        void Debug(string message);

        /// <summary>
        /// Записывает в лог уровня Debug структурное сообщение
        /// </summary>
        /// <typeparam name="T">Тип аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property">Аргумент</param>
        void Debug<T>(string messageTemplate, T property);

        /// <summary>
        /// Записывает в лог уровня Debug структурное сообщение
        /// </summary>
        /// <typeparam name="T0">Тип первого аргумента</typeparam>
        /// <typeparam name="T1">Тип второго аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property0">Первый аргумент</param>
        /// <param name="property1">Второй аргумент</param>
        void Debug<T0, T1>(string messageTemplate, T0 property0, T1 property1);

        /// <summary>
        /// Записывает в лог уровня Debug структурное сообщение
        /// </summary>
        /// <typeparam name="T0">Тип первого аргумента</typeparam>
        /// <typeparam name="T1">Тип второго аргумента</typeparam>
        /// <typeparam name="T2">ТИп третьего аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property0">Первый аргумент</param>
        /// <param name="property1">Второй аргумент</param>
        /// <param name="property2">Третий аргумент</param>
        void Debug<T0, T1, T2>(string messageTemplate, T0 property0, T1 property1, T2 property2);

        /// <summary>
        /// Записывает в лог уровня Debug структурное сообщение
        /// </summary>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="properties">Аргументы</param>
        void Debug(string messageTemplate, params object[] properties);

        #endregion

        #region Trace

        /// <summary>
        /// Записывает в лог уровня Trace
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        void Trace(string message);

        /// <summary>
        /// Записывает в лог уровня Trace структурное сообщение
        /// </summary>
        /// <typeparam name="T">Тип аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property">Аргумент</param>
        void Trace<T>(string messageTemplate, T property);

        /// <summary>
        /// Записывает в лог уровня Trace структурное сообщение
        /// </summary>
        /// <typeparam name="T0">Тип первого аргумента</typeparam>
        /// <typeparam name="T1">Тип второго аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property0">Первый аргумент</param>
        /// <param name="property1">Второй аргумент</param>
        void Trace<T0, T1>(string messageTemplate, T0 property0, T1 property1);

        /// <summary>
        /// Записывает в лог уровня Trace структурное сообщение
        /// </summary>
        /// <typeparam name="T0">Тип первого аргумента</typeparam>
        /// <typeparam name="T1">Тип второго аргумента</typeparam>
        /// <typeparam name="T2">ТИп третьего аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property0">Первый аргумент</param>
        /// <param name="property1">Второй аргумент</param>
        /// <param name="property2">Третий аргумент</param>
        void Trace<T0, T1, T2>(string messageTemplate, T0 property0, T1 property1, T2 property2);

        /// <summary>
        /// Записывает в лог уровня Trace структурное сообщение
        /// </summary>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="properties">Аргументы</param>
        void Trace(string messageTemplate, params object[] properties);

        #endregion

        #region Info

        /// <summary>
        /// Записывает в лог уровня Info
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        void Info(string message);

        /// <summary>
        /// Записывает в лог уровня Info структурное сообщение
        /// </summary>
        /// <typeparam name="T">Тип аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property">Аргумент</param>
        void Info<T>(string messageTemplate, T property);

        /// <summary>
        /// Записывает в лог уровня Info структурное сообщение
        /// </summary>
        /// <typeparam name="T0">Тип первого аргумента</typeparam>
        /// <typeparam name="T1">Тип второго аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property0">Первый аргумент</param>
        /// <param name="property1">Второй аргумент</param>
        void Info<T0, T1>(string messageTemplate, T0 property0, T1 property1);

        /// <summary>
        /// Записывает в лог уровня Info структурное сообщение
        /// </summary>
        /// <typeparam name="T0">Тип первого аргумента</typeparam>
        /// <typeparam name="T1">Тип второго аргумента</typeparam>
        /// <typeparam name="T2">ТИп третьего аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property0">Первый аргумент</param>
        /// <param name="property1">Второй аргумент</param>
        /// <param name="property2">Третий аргумент</param>
        void Info<T0, T1, T2>(string messageTemplate, T0 property0, T1 property1, T2 property2);

        /// <summary>
        /// Записывает в лог уровня Info структурное сообщение
        /// </summary>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="properties">Аргументы</param>
        void Info(string messageTemplate, params object[] properties);

        #endregion

        #region Warning

        /// <summary>
        /// Записывает в лог уровня Warning
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        void Warning(string message);

        /// <summary>
        /// Записывает в лог уровня Warning структурное сообщение
        /// </summary>
        /// <typeparam name="T">Тип аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property">Аргумент</param>
        void Warning<T>(string messageTemplate, T property);

        /// <summary>
        /// Записывает в лог уровня Warning структурное сообщение
        /// </summary>
        /// <typeparam name="T0">Тип первого аргумента</typeparam>
        /// <typeparam name="T1">Тип второго аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property0">Первый аргумент</param>
        /// <param name="property1">Второй аргумент</param>
        void Warning<T0, T1>(string messageTemplate, T0 property0, T1 property1);

        /// <summary>
        /// Записывает в лог уровня Warning структурное сообщение
        /// </summary>
        /// <typeparam name="T0">Тип первого аргумента</typeparam>
        /// <typeparam name="T1">Тип второго аргумента</typeparam>
        /// <typeparam name="T2">ТИп третьего аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property0">Первый аргумент</param>
        /// <param name="property1">Второй аргумент</param>
        /// <param name="property2">Третий аргумент</param>
        void Warning<T0, T1, T2>(string messageTemplate, T0 property0, T1 property1, T2 property2);

        /// <summary>
        /// Записывает в лог уровня Warning структурное сообщение
        /// </summary>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="properties">Аргументы</param>
        void Warning(string messageTemplate, params object[] properties);

        /// <summary>
        /// Записывает в лог уровня Warning информацию об исключении
        /// </summary>
        /// <param name="exception">Исключение</param>
        void Warning(Exception exception);

        /// <summary>
        /// Записывает в лог уровня Warning информацию об исключении
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception">Исключение</param>
        void Warning(string message, Exception exception);

        /// <summary>
        /// Записывает в лог уровня Warning информацию об исключении в структурированном виде
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception">Исключение</param>
        /// <param name="properties">Аргументы></param>
        void Warning(string message, Exception exception, params object[] properties);

        #endregion

        #region Error

        /// <summary>
        /// Записывает в лог уровня Error
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        void Error(string message);

        /// <summary>
        /// Записывает в лог уровня Error структурное сообщение
        /// </summary>
        /// <typeparam name="T">Тип аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property">Аргумент</param>
        void Error<T>(string messageTemplate, T property);

        /// <summary>
        /// Записывает в лог уровня Error структурное сообщение
        /// </summary>
        /// <typeparam name="T0">Тип первого аргумента</typeparam>
        /// <typeparam name="T1">Тип второго аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property0">Первый аргумент</param>
        /// <param name="property1">Второй аргумент</param>
        void Error<T0, T1>(string messageTemplate, T0 property0, T1 property1);

        /// <summary>
        /// Записывает в лог уровня Error структурное сообщение
        /// </summary>
        /// <typeparam name="T0">Тип первого аргумента</typeparam>
        /// <typeparam name="T1">Тип второго аргумента</typeparam>
        /// <typeparam name="T2">ТИп третьего аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property0">Первый аргумент</param>
        /// <param name="property1">Второй аргумент</param>
        /// <param name="property2">Третий аргумент</param>
        void Error<T0, T1, T2>(string messageTemplate, T0 property0, T1 property1, T2 property2);

        /// <summary>
        /// Записывает в лог уровня Error структурное сообщение
        /// </summary>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="properties">Аргументы</param>
        void Error(string messageTemplate, params object[] properties);

        /// <summary>
        /// Записывает в лог уровня Error информацию об исключении
        /// </summary>
        /// <param name="exception">Исключение</param>
        void Error(Exception exception);

        /// <summary>
        /// Записывает в лог уровня Error информацию об исключении
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception">Исключение</param>
        void Error(string message, Exception exception);

        /// <summary>
        /// Записывает в лог уровня Error информацию об исключении в структурированном виде
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception">Исключение</param>
        /// <param name="properties">Аргументы></param>
        void Error(string message, Exception exception, params object[] properties);

        #endregion

    }
}
