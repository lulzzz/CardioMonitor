using System;
using Scout.Utils.Logging;

namespace CardioMonitor.Infrastructure.Workers
{
    public interface IWorkerController
    {
        
        
        void SetDefaultLogger(ILogger logger);
        Worker StartWorker(TimeSpan period, Action workMethod);
        Worker StartWorker(TimeSpan period, Action workMethod, ILogger logger);
        Worker StartWorker(TimeSpan period, TimeSpan firstTimePeriod, Action workMethod);
        Worker StartWorker(TimeSpan period, TimeSpan firstTimePeriod, Action workMethod, ILogger logger);
        void StopAllWorkers();
        void CloseWorker(Worker worker);
        Worker<T> StartWorker<T>(TimeSpan period, Action<T> workMethod, T state);
        Worker<T> StartWorker<T>(TimeSpan period, Action<T> workMethod, T state, ILogger logger);
        Worker<T> StartWorker<T>(TimeSpan period, TimeSpan firstTimePeriod, Action<T> workMethod, T state);
        Worker<T> StartWorker<T>(TimeSpan period, TimeSpan firstTimePeriod, Action<T> workMethod, T state, ILogger logger);
    }
}