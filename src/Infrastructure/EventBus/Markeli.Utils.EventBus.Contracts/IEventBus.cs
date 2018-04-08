using System;
using System.Threading.Tasks;

namespace Scout.Utils.EventBus.Contracts
{
    /// <summary>
    /// Шина событий
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Асинхронно событие публикует в шину 
        /// </summary>
        /// <typeparam name="TEvent">Типа событие</typeparam>
        /// <param name="event">Само событие</param>
        /// <returns></returns>
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent;

        /// <summary>
        /// Публикует событие в шину 
        /// </summary>
        /// <typeparam name="TEvent">Типа событие</typeparam>
        /// <param name="event">Само событие</param>
        /// <returns></returns>
        void Publish<TEvent>(TEvent @event) where TEvent : IEvent;

        /// <summary>
        /// Возвращает все события определенного типа, опубликованные в шине
        /// </summary>
        /// <typeparam name="TEvent">Тип события</typeparam>
        /// <returns>Все опубликованные события нужного типа</returns>
        IObservable<TEvent> GetEvents<TEvent>() where TEvent: IEvent;
    }
}