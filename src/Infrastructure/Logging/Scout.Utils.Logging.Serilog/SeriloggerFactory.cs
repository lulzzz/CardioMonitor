using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Scout.Utils.Logging.Serilog
{
    /// <summary>
    /// Фабрика по созданию фасада логера Serilog
    /// </summary>
    public class SeriloggerFactory : ILoggerFactory
    {
        private readonly ISerilogConfigurator _serilogConfigurator;
     
        /// <summary>
        /// Для конфигурирования серилога можно пользоваться конфигурационным файлом (свойство ConfigPath), FluentAPI (метод ConfigureSerilog),
        /// так и сочетать ба варианта. 
        /// </summary>
        /// <param name="serilogConfigurator">Класс с конфигурацией логера</param>
        public SeriloggerFactory(ISerilogConfigurator serilogConfigurator)
        {
            if(serilogConfigurator == null) throw new ArgumentNullException();
            
            _serilogConfigurator = serilogConfigurator;
        }

        /// <summary>
        /// Создает логгер
        /// </summary>
        /// <param name="loggerName">Имя логера</param>
        /// <param name="correlationId">Id который будет содержаться во всех сообщениях данного логера (для структурного логирования)</param>
        /// <returns>Логер</returns>
        public ILogger CreateLogger(string loggerName, string correlationId = null)
        {
            var configuration = new LoggerConfiguration();

            configuration
                .Enrich.WithProperty(DefaultPropertyNames.LoggerNamePropertyName, loggerName);
            
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                configuration
                    .Enrich.WithProperty(DefaultPropertyNames.CorrelationIdPropertyName, correlationId);
            }
            
            if (!string.IsNullOrWhiteSpace(_serilogConfigurator.ConfigPath))
            {
                configuration
                    .ReadFrom.AppSettings(null, _serilogConfigurator.ConfigPath);
            }
            
            _serilogConfigurator.ConfigureSerilog(configuration);
            configuration.MinimumLevel.Verbose();
            
            var logger = configuration.CreateLogger();
            
            return new SeriLogger(loggerName, logger);
        }
    }
}