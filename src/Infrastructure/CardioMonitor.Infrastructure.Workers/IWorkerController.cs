using System;
using Common.Logging;

namespace CardioMonitor.Infrastructure.Workers
{
    public interface IWorkerController
    {
        
        
        void SetDefaultLogger(ILog logger);
        Worker StartWorker(TimeSpan period, Action workMethod);
        Worker StartWorker(TimeSpan period, Action workMethod, ILog logger);
        Worker StartWorker(TimeSpan period, TimeSpan firstTimePeriod, Action workMethod);
        Worker StartWorker(TimeSpan period, TimeSpan firstTimePeriod, Action workMethod, ILog logger);
        void StopAllWorkers();
        void CloseWorker(Worker worker);
        Worker<T> StartWorker<T>(TimeSpan period, Action<T> workMethod, T state);
        Worker<T> StartWorker<T>(TimeSpan period, Action<T> workMethod, T state, ILog logger);
        Worker<T> StartWorker<T>(TimeSpan period, TimeSpan firstTimePeriod, Action<T> workMethod, T state);
        Worker<T> StartWorker<T>(TimeSpan period, TimeSpan firstTimePeriod, Action<T> workMethod, T state, ILog logger);
    }
}