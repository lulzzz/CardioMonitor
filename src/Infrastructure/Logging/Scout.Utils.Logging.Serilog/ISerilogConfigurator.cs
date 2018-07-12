using Serilog;

namespace Scout.Utils.Logging.Serilog
{
    /// <summary>
    /// Интерфейс позволяющий пользователям SeriloggerFactory настроить логер с помощью Fluent API. 
    /// О настройке логера с использованием класса LoggerConfiguration можно прочесть по сылке: 
    /// https://github.com/serilog/serilog/wiki/Configuration-Basics
    /// </summary>
    public interface ISerilogConfigurator
    {
        /// <summary>
        /// Настройки Serilog c пом-ю FluentAPI. 
        /// </summary>
        /// <remarks>Если все настройки в конфиге - просто оставить пустым</remarks>>
        /// <param name="configuration"></param>
        void ConfigureSerilog(LoggerConfiguration configuration);
        
        /// <summary>
        /// Путь к файлу с конфигурацией Serilog
        /// </summary>
        string ConfigPath { get; }
    }
}