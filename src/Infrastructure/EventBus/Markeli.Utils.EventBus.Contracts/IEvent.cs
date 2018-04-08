using System;

namespace Scout.Utils.EventBus.Contracts
{
    /// <summary>
    /// Событие
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Уникальный идентфикатор самого события
        /// </summary>
        Guid Id { get; }
        
        /// <summary>
        /// Уникальный идентификатор типа события
        /// </summary>
        Guid TypeId { get; }
    }
}