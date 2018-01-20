using System;

namespace Scout.Utils.Logging
{
    /// <summary>
    /// Интерфейс фабрики по созданию логеров
    /// </summary>
    /// <remarks>
    /// Все тонкости конфигурирования должны быть скрыты тут
    /// </remarks>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Создает логгер
        /// </summary>
        /// <param name="loggerName">Имя логера</param>
        /// <param name="correlationId">Id который будет содержаться во всех сообщениях данного логера (для структурного логирования)</param>
        /// <returns>Логер</returns>
        ILogger CreateLogger(string loggerName, string correlationId = null);
    }
}
