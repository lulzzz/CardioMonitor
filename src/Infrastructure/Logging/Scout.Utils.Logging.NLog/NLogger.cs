using System;
using NLog;

namespace Scout.Utils.Logging.NLog
{
    /// <summary>
    /// Фасад логера над NLog
    /// </summary>
    public class NLogger : ILogger
    {
        private readonly Logger _logger;

        /// <summary>
        /// Имя логера
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Фасад логера над NLog
        /// </summary>
        /// <param name="logger">Логгер NLog</param>
        public NLogger(Logger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            Name = logger.Name;
            _logger = logger;
        }

        #region Debug

        /// <summary>
        /// Записывает в лог уровня Debug
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        /// <summary>
        /// Записывает в лог уровня Debug структурное сообщение
        /// </summary>
        /// <typeparam name="T">Тип аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property">Аргумент</param>
        public void Debug<T>(string messageTemplate, T property)
        {
            _logger.Debug(messageTemplate, property);
        }

        /// <summary>
        /// Записывает в лог уровня Debug структурное сообщение
        /// </summary>
        /// <typeparam name="T0">Тип первого аргумента</typeparam>
        /// <typeparam name="T1">Тип второго аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property0">Первый аргумент</param>
        /// <param name="property1">Второй аргумент</param>
        public void Debug<T0, T1>(string messageTemplate, T0 property0, T1 property1)
        {
            _logger.Debug(messageTemplate, property0, property1);
        }

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
        public void Debug<T0, T1, T2>(string messageTemplate, T0 property0, T1 property1, T2 property2)
        {
            _logger.Debug(messageTemplate, property0, property1, property2);
        }

        /// <summary>
        /// Записывает в лог уровня Debug структурное сообщение
        /// </summary>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="properties">Аргументы</param>
        public void Debug(string messageTemplate, params object[] properties)
        {
            _logger.Debug(messageTemplate, properties);
        }

        #endregion

        #region Trace

        /// <summary>
        /// Записывает в лог уровня Trace
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        public void Trace(string message)
        {
            _logger.Trace(message);
        }

        /// <summary>
        /// Записывает в лог уровня Trace структурное сообщение
        /// </summary>
        /// <typeparam name="T">Тип аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property">Аргумент</param>
        public void Trace<T>(string messageTemplate, T property)
        {
            _logger.Trace(messageTemplate, property);
        }

        /// <summary>
        /// Записывает в лог уровня Trace структурное сообщение
        /// </summary>
        /// <typeparam name="T0">Тип первого аргумента</typeparam>
        /// <typeparam name="T1">Тип второго аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property0">Первый аргумент</param>
        /// <param name="property1">Второй аргумент</param>
        public void Trace<T0, T1>(string messageTemplate, T0 property0, T1 property1)
        {
            _logger.Trace(messageTemplate, property0, property1);
        }

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
        public void Trace<T0, T1, T2>(string messageTemplate, T0 property0, T1 property1, T2 property2)
        {
            _logger.Trace(messageTemplate, property0, property1, property2);
        }

        /// <summary>
        /// Записывает в лог уровня Trace структурное сообщение
        /// </summary>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="properties">Аргументы</param>
        public void Trace(string messageTemplate, params object[] properties)
        {
            _logger.Trace(messageTemplate, properties);
        }

        #endregion

        #region Info

        /// <summary>
        /// Записывает в лог уровня Info
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        public void Info(string message)
        {
            _logger.Info(message);
        }

        /// <summary>
        /// Записывает в лог уровня Info структурное сообщение
        /// </summary>
        /// <typeparam name="T">Тип аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property">Аргумент</param>
        public void Info<T>(string messageTemplate, T property)
        {
            _logger.Info(messageTemplate, property);
        }

        /// <summary>
        /// Записывает в лог уровня Info структурное сообщение
        /// </summary>
        /// <typeparam name="T0">Тип первого аргумента</typeparam>
        /// <typeparam name="T1">Тип второго аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property0">Первый аргумент</param>
        /// <param name="property1">Второй аргумент</param>
        public void Info<T0, T1>(string messageTemplate, T0 property0, T1 property1)
        {
            _logger.Info(messageTemplate, property0, property1);
        }

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
        public void Info<T0, T1, T2>(string messageTemplate, T0 property0, T1 property1, T2 property2)
        {
            _logger.Info(messageTemplate, property0, property1, property2);
        }

        /// <summary>
        /// Записывает в лог уровня Info структурное сообщение
        /// </summary>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="properties">Аргументы</param>
        public void Info(string messageTemplate, params object[] properties)
        {
            _logger.Info(messageTemplate, properties);
        }

        #endregion

        #region Warning

        /// <summary>
        /// Записывает в лог уровня Warning
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        public void Warning(string message)
        {
            _logger.Warn(message);
        }

        /// <summary>
        /// Записывает в лог уровня Warning структурное сообщение
        /// </summary>
        /// <typeparam name="T">Тип аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property">Аргумент</param>
        public void Warning<T>(string messageTemplate, T property)
        {
            _logger.Warn(messageTemplate, property);
        }

        /// <summary>
        /// Записывает в лог уровня Warning структурное сообщение
        /// </summary>
        /// <typeparam name="T0">Тип первого аргумента</typeparam>
        /// <typeparam name="T1">Тип второго аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property0">Первый аргумент</param>
        /// <param name="property1">Второй аргумент</param>
        public void Warning<T0, T1>(string messageTemplate, T0 property0, T1 property1)
        {
            _logger.Warn(messageTemplate, property0, property1);
        }

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
        public void Warning<T0, T1, T2>(string messageTemplate, T0 property0, T1 property1, T2 property2)
        {
            _logger.Warn(messageTemplate, property0, property1, property2);
        }

        /// <summary>
        /// Записывает в лог уровня Warning структурное сообщение
        /// </summary>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="properties">Аргументы</param>
        public void Warning(string messageTemplate, params object[] properties)
        {
            _logger.Warn(messageTemplate, properties);
        }

        /// <summary>
        /// Записывает в лог уровня Warning информацию об исключении
        /// </summary>
        /// <param name="exception">Исключение</param>
        public void Warning(Exception exception)
        {
            _logger.Warn(exception);
        }

        /// <summary>
        /// Записывает в лог уровня Warning информацию об исключении
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception">Исключение</param>
        public void Warning(string message, Exception exception)
        {
            _logger.Warn(exception, message);
        }

        /// <summary>
        /// Записывает в лог уровня Warning информацию об исключении в структурированном виде
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception">Исключение</param>
        /// <param name="properties">Аргументы></param>
        public void Warning(string message, Exception exception, params object[] properties)
        {
            _logger.Warn(exception, message, properties);
        }

        #endregion

        #region Error

        /// <summary>
        /// Записывает в лог уровня Error
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        public void Error(string message)
        {
            _logger.Error(message);
        }

        /// <summary>
        /// Записывает в лог уровня Error структурное сообщение
        /// </summary>
        /// <typeparam name="T">Тип аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property">Аргумент</param>
        public void Error<T>(string messageTemplate, T property)
        {
            _logger.Error(messageTemplate, property);
        }
        
        /// <summary>
        /// Записывает в лог уровня Error структурное сообщение
        /// </summary>
        /// <typeparam name="T0">Тип первого аргумента</typeparam>
        /// <typeparam name="T1">Тип второго аргумента</typeparam>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="property0">Первый аргумент</param>
        /// <param name="property1">Второй аргумент</param>
        public void Error<T0, T1>(string messageTemplate, T0 property0, T1 property1)
        {
            _logger.Error(messageTemplate, property0, property1);
        }

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
        public void Error<T0, T1, T2>(string messageTemplate, T0 property0, T1 property1, T2 property2)
        {
            _logger.Error(messageTemplate, property0, property1, property2);
        }

        /// <summary>
        /// Записывает в лог уровня Error структурное сообщение
        /// </summary>
        /// <param name="messageTemplate">Шаблон сообщения</param>
        /// <param name="properties">Аргументы</param>
        public void Error(string messageTemplate, params object[] properties)
        {
            _logger.Error(messageTemplate, properties);
        }

        /// <summary>
        /// Записывает в лог уровня Error информацию об исключении
        /// </summary>
        /// <param name="exception">Исключение</param>
        public void Error(Exception exception)
        {
            _logger.Error(exception);
        }

        /// <summary>
        /// Записывает в лог уровня Error информацию об исключении
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception">Исключение</param>
        public void Error(string message, Exception exception)
        {
            _logger.Error(exception, message);
        }

        /// <summary>
        /// Записывает в лог уровня Error информацию об исключении в структурированном виде
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception">Исключение</param>
        /// <param name="properties">Аргументы></param>
        public void Error(string message, Exception exception, params object[] properties)
        {
            _logger.Error(exception, message, properties);
        }


        #endregion
    }
}