using System;
using System.Collections.Generic;
using System.Linq;
using Markeli.Utils.Logging;

namespace CardioMonitor.Infrastructure.Workers
{
    public class WorkerController : IWorkerController
    {
        public WorkerController()
        {
            _workers = new Dictionary<Guid, Worker>();
            _locker = new object();
        }

        private readonly Dictionary<Guid, Worker> _workers;
        private readonly object _locker;
        private ILogger _logger;

        private Worker RegisterWorker(TimeSpan period, Action workMethod, ILogger logger)
        {
            var id = Guid.NewGuid();
            var worker = new Worker(id, period, workMethod, logger);
            lock (_locker)
            {
                _workers[id] = worker;
            }
            return worker;
        }


        public void SetDefaultLogger(ILogger logger)
        {
            _logger = logger;
        }

        public Worker StartWorker(TimeSpan period, Action workMethod)
        {
            var worker = RegisterWorker(period, workMethod, _logger);
            worker?.Start();
            return worker;
        }

        public Worker StartWorker(TimeSpan period, Action workMethod, ILogger logger)
        {
            var worker = RegisterWorker(period, workMethod, logger);
            worker?.Start();
            return worker;
        }

        public Worker StartWorker(TimeSpan period, TimeSpan firstTimePeriod, Action workMethod)
        {
            var worker = RegisterWorker(period, workMethod, _logger);
            worker?.Start(firstTimePeriod);
            return worker;
        }

        public Worker StartWorker(TimeSpan period, TimeSpan firstTimePeriod, Action workMethod, ILogger logger)
        {
            var worker = RegisterWorker(period, workMethod, logger);
            worker?.Start(firstTimePeriod);
            return worker;
        }

        private void UnregisterWorker(Worker worker)
        {
            if (worker == null) return;
            lock (_locker)
            {
                if (_workers.ContainsKey(worker.Id))
                {
                    _workers.Remove(worker.Id);
                }
            }
        }

        public void StopAllWorkers()
        {
            var workers = _workers.Values.ToArray();
            foreach (var worker in workers)
            {
                if (worker == null) continue;
                
                worker.Stop();
                UnregisterWorker(worker);
            }
        }

        public void CloseWorker(Worker worker)
        {
            if (worker == null) return;
            worker.Stop();
            UnregisterWorker(worker);
        }
        
        private Worker<T> RegisterWorker<T>(TimeSpan period, Action<T> workMethod, T state, ILogger logger)
        {
            var id = Guid.NewGuid();
            var worker = new Worker<T>(id, period, workMethod, state, logger);
            lock (_locker)
            {
                _workers[id] = worker;
            }
            return worker;
        }

        public Worker<T> StartWorker<T>(TimeSpan period, Action<T> workMethod, T state)
        {
            var worker = RegisterWorker(period, workMethod, state, _logger);
            worker?.Start();
            return worker;
        }
        public Worker<T> StartWorker<T>(TimeSpan period, Action<T> workMethod, T state, ILogger logger)
        {
            var worker = RegisterWorker(period, workMethod, state, logger);
            worker?.Start();
            return worker;
        }

        public Worker<T> StartWorker<T>(TimeSpan period, TimeSpan firstTimePeriod, Action<T> workMethod, T state)
        {
            var worker = RegisterWorker(period, workMethod, state, _logger);
            worker?.Start(firstTimePeriod);
            return worker;
        }
        public Worker<T> StartWorker<T>(TimeSpan period, TimeSpan firstTimePeriod, Action<T> workMethod, T state, ILogger logger)
        {
            var worker = RegisterWorker(period, workMethod, state, logger);
            worker?.Start(firstTimePeriod);
            return worker;
        }
    }
}