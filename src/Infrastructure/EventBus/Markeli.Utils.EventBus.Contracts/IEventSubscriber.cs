using System;

namespace Markeli.Utils.EventBus.Contracts
{
    /// <summary>
    /// Подписчик на события
    /// </summary>
    public interface IEventSubscriber : IDisposable
    {
        /// <summary>
        /// Подписаться на получение события
        /// </summary>
        void Subscribe();
        
        /// <summary>
        /// Отписаться от получения события
        /// </summary>
        void Unsubscribe();
    }
}