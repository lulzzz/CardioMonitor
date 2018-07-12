using System;
using Markeli.Utils.EventBus.Contracts;

namespace Markeli.Utils.EventBus.Local
{
   
    /// <summary>
    /// Базовый подписчик на события на сервере
    /// </summary>
    /// <remarks>
    /// Если вам нужен подписчик на все события, то достаточно отнаследоваться от этого базового класса и переопределить Consume
    /// </remarks>
    /// <typeparam name="TEvent">Тип события, на который надо подписаться</typeparam>
    public abstract class BaseLocalEventSubscriber<TEvent> 
        : IEventSubscriber where TEvent : IEvent
    {
        protected readonly IEventBus EventBus;
        protected IDisposable Observer;
        protected  IObservable<TEvent> Observable;

        /// <summary>
        /// Базовый подписчик на события
        /// </summary>
        protected BaseLocalEventSubscriber(IEventBus eventBus)
        {
            EventBus = eventBus;
            // ReSharper disable once VirtualMemberCallInContructor
            InitObservable();
        }

        protected virtual void InitObservable()
        {
            Observable = EventBus.GetEvents<TEvent>();
        }

        public virtual void Subscribe()
        {
            Observer = Observable?.Subscribe(Consume);
        }

        public virtual void Unsubscribe()
        {
            Dispose();
        }

        /// <summary>
        /// Вызывается при публикации в шине нужной нотификации
        /// </summary>
        /// <param name="event"></param>
        protected abstract void Consume(TEvent @event);

        public void Dispose()
        {
            Observer?.Dispose();
        }
    }
}