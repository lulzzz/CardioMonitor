using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Markeli.Utils.EventBus.Contracts;

namespace Markeli.Utils.EventBus.Local
{
    /// <summary>
    /// Локальная для сервера шина событий
    /// </summary>
    public class LocalEventBus : ILocalEventBus
    {
        private readonly ConcurrentDictionary<Type, object> _subjects = new ConcurrentDictionary<Type, object>();

        public IObservable<TEvent> GetEvents<TEvent>() where TEvent : IEvent
        {
            var subject = (ISubject<TEvent>)_subjects.GetOrAdd(typeof(TEvent), x => new Subject<TEvent>());
            return subject.AsObservable();
        }

        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
           return Task.Factory.StartNew(n =>
           {
               Publish((TEvent)n);
           }, @event, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        public void Publish<TEvent>(TEvent @event) where TEvent : IEvent
        {
            if (_subjects.TryGetValue(typeof(TEvent), out var subject))
            {
                ((ISubject<TEvent>)subject).OnNext(@event);
            }
        }
    }
}
