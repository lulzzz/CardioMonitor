using System;
using NLog;
using NLogInternal = NLog;

namespace Scout.Utils.Logging.ScoutNLog
{
    /// <summary>
    /// Фабрика по созданию фасада логера NLog
    /// </summary>
    public class NLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// Фабрика по созданию фасада логера NLog
        /// </summary>
        /// <param name="configPath">Путь к конфигурационному файла NLog</param>
        public NLoggerFactory(string configPath)
        {
            LogManager.Configuration = new NLogInternal.Config.XmlLoggingConfiguration(configPath);
        }

        /// <summary>
        /// Создает логер с заданным именем
        /// </summary>
        /// <param name="loggerName">Имя логера</param>
        /// <returns>Готовый к использованию логер</returns>
        /// <param name="correlationId"> не используется</param>
        public ILogger CreateLogger(string loggerName, string correlationId = null)
        {
            if (loggerName == null) throw new ArgumentNullException(nameof(loggerName));

            var logger = LogManager.GetLogger(loggerName);
          
            return new NLogger(logger);
        }
    }
}