using System;
using System.ComponentModel;
using System.Threading;

namespace CardioMonitor.Infrastructure.Threading
{
    /// <summary>
    /// Таймер, обертка вокруг BackgroundWorker с возможностью приостанавливать и возобновлять работу таймера
    /// </summary>
    public class CardioTimer
    {
        private const double Tolerance = 0.0001;

        private bool _isThreadSuspended;
        private readonly EventHandler _timerEvent;
        private TimeSpan _workTime;
        private BackgroundWorker _worker;
        private readonly TimeSpan _period;

        /// <summary>
        /// Таймер, обертка вокруг BackgroundWorker с возможностью приостанавливать выполнение задачи
        /// </summary>
        /// <param name="timerEvent">Событие, которое будет вызывать таймер</param>
        /// <param name="workTime">Время работы таймера</param>
        /// <param name="period">Период, в течение которого будет вызваться событие timerEvent</param>
        public CardioTimer(EventHandler timerEvent, TimeSpan workTime, TimeSpan period)
        {
            if (null == timerEvent) { throw new ArgumentNullException(nameof(timerEvent)); }
            if (null == workTime) { throw new ArgumentNullException(nameof(workTime));}
            if (null == period) { throw new ArgumentNullException(nameof(period));}

            _timerEvent = timerEvent;
            _workTime = workTime;
            _period = period;
            _worker = new BackgroundWorker { WorkerSupportsCancellation = true };
            _worker.DoWork += TimerWork;
        }

        /// <summary>
        /// Выполняет вызов события timerEvent с периодичностью period в течение workTime
        /// </summary>
        /// <param name="sender">Запускающий объект</param>
        /// <param name="e">Параметры запуска</param>
        private void TimerWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            if (null == worker) { return;}
            do
            {
                Thread.Sleep(_period);
                if (!_isThreadSuspended)
                {
                    _workTime -= _period;
                    //Task.Factory.StartNew(() => _timerEvent(null, null));
                    _timerEvent(null, null);
                }
            }
            while (!worker.CancellationPending && Math.Abs(_workTime.TotalSeconds) > Tolerance);
        }

        /// <summary>
        /// Запускает таймер
        /// </summary>
        public void Start()
        {
            _isThreadSuspended = false;
            _worker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _worker.DoWork += TimerWork;
            _worker.RunWorkerAsync();
        }

        /// <summary>
        /// Прекращаешь работу таймера
        /// </summary>
        public void Stop()
        {
            if (null != _worker)
            {
                _worker.CancelAsync();
                _worker.Dispose();
                _worker = null;
            }
            _isThreadSuspended = false;
        }

        /// <summary>
        /// Приостанавливает работу таймера
        /// </summary>
        public void Suspend()
        {
            _isThreadSuspended = true;
        }

        /// <summary>
        /// Продолжает работу таймеру после приостановки
        /// </summary>
        public void Resume()
        {
            _isThreadSuspended = false;
        }
    }
}
