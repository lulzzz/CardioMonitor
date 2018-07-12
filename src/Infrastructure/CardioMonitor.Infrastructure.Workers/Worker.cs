using System;
using System.Threading;
using Markeli.Utils.Logging;

namespace CardioMonitor.Infrastructure.Workers
{
    public class Worker
    {
        public Guid Id { get; }
        public TimeSpan Period { get; }
        private readonly Action _workMethod;
        private readonly ILogger _logger;
        private readonly TimerCallback _timerMethod;
        private readonly int _periodMs;
        private Timer _timer;
        private readonly object _locker;

        public Worker(Guid id, TimeSpan period, Action workMethod, ILogger logger)
        {
            Id = id;
            Period = period;
            _workMethod = workMethod;
            _logger = logger;
            _timerMethod = WorkMethod;
            _locker = new object();
            _periodMs = (int) period.TotalMilliseconds;
        }

        protected Worker(Guid id, TimeSpan period)
        {
            Id = id;
            Period = period;
            _timerMethod = WorkMethod;
            _locker = new object();
            _periodMs = (int) period.TotalMilliseconds;
        }

        public void Start(TimeSpan? firstTimePeriod = null)
        {
            var timerMethod = _timerMethod;
            if (timerMethod == null) return;
            _timer = new Timer(timerMethod, null,
                firstTimePeriod.HasValue ? (int) firstTimePeriod.Value.TotalMilliseconds : _periodMs, Timeout.Infinite);
        }

        public void Stop()
        {
            var timer = _timer;
            if (timer != null)
            {
                lock (_locker)
                {
                    try
                    {
                        timer.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger?.Error($"{nameof(Stop)} error", ex);
                    }
                }
            }
            _timer = null;
        }

        protected virtual void OnWork()
        {
            var workMethod = _workMethod;
            if (workMethod != null)
            {
                try
                {
                    workMethod();
                }
                catch (Exception ex)
                {
                    // ReSharper disable once InconsistentlySynchronizedField
                    _logger?.Error( $"{nameof(OnWork)} error", ex);
                }
            }
        }

        private void WorkMethod(object state)
        {
            OnWork();
            var timer = _timer;
            if (timer != null)
            {
                lock (_locker)
                {
                    try
                    {
                        timer.Change(_periodMs, Timeout.Infinite);
                    }
                    catch (Exception ex)
                    {
                        _logger?.Error($"{nameof(WorkMethod)} error", ex);
                    }
                }
            }
        }
    }

    public sealed class Worker<T> : Worker
    {
        private readonly Action<T> _workMethod;
        private readonly ILogger _logger;
        public T State { get; set; }

        public Worker(Guid id, TimeSpan period, Action<T> workMethod, T state, ILogger logger)
            : base(id, period)
        {
            State = state;
            _workMethod = workMethod;
            _logger = logger;
        }

        protected override void OnWork()
        {
            var workMethod = _workMethod;
            if (workMethod != null)
            {
                try
                {
                    workMethod(State);
                }
                catch (Exception ex)
                {
                    _logger?.Error($"{nameof(OnWork)} error", ex);
                }
            }
        }
    }
}