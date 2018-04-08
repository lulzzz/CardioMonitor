using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Infrastructure.Workers;

namespace CardioMonitor.Devices.Bed.Fake
{
    public class FakeBedController : IBedController
    {
        private readonly IWorkerController _workerController;
        private FakeDeviceInitParams _initParams;
        private Worker _worker;
        private const float MaxAngleX = 31.5f;
        private const short IterationsCountOnMaxAngleX = 40;
        private const short CheckPointsCountOnMaxAngleX = 5;

        private TimeSpan _sessionDuration;
        private TimeSpan _elapsedTime;
        private TimeSpan _cycleDuration;
        private short _iterationsCount;
        private short _checkPointsCount;
        private ICollection<short> _checkPointIterationNumber;

        public void Dispose()
        {
        }

        public event EventHandler OnPauseFromDeviceRequested;
        public event EventHandler OnResumeFromDeviceRequested;
        public event EventHandler OnEmeregencyStopFromDeviceRequested;
        public event EventHandler OnReverseFromDeviceRequested;

        public FakeBedController(IWorkerController workerController)
        {
            _workerController = workerController;
            _elapsedTime = TimeSpan.Zero;
        }

        public bool IsConnected { get; private set; }

        public void Init(IBedControllerInitParams initParams)
        {
            if (!(initParams is FakeDeviceInitParams localParms)) throw new ArgumentException();

            _initParams = localParms;
            Init();
        }

        public Task ConnectAsync()
        {
            IsConnected = true;
            _worker = _workerController.StartWorker(_initParams.UpdateDataPeriod, WorkMethod);
            
            return Task.Delay(_initParams.ConenctDelay);
        }

        private void Init()
        {
            _cycleDuration = GetCycleDuration();
            _sessionDuration = TimeSpan.FromTicks(_cycleDuration.Ticks * _initParams.CyclesCount);
            _iterationsCount = GetIterationsCount();

            _checkPointsCount = (short)Math.Round(CheckPointsCountOnMaxAngleX * _initParams.MaxAngleX / MaxAngleX);
            _checkPointIterationNumber = new List<short>(_checkPointsCount);

            var iterationPerCheckPoint = _iterationsCount / _checkPointsCount;

            var checkPointIteration = 0;

            for (var i = 0; i < _checkPointsCount; ++i)
            {
                checkPointIteration += iterationPerCheckPoint;
                _checkPointIterationNumber.Add((short)checkPointIteration);
            }
        }


        private TimeSpan GetCycleDuration()
        {
            var ticks = (long)Math.Round(_initParams.CycleWithMaxAngleDuration.Ticks / MaxAngleX * _initParams.MaxAngleX);
            return TimeSpan.FromTicks(ticks);
        }

        private short GetIterationsCount()
        {
            return (short)Math.Round(IterationsCountOnMaxAngleX / MaxAngleX * _initParams.MaxAngleX);
        }

        private void WorkMethod()
        {
            _elapsedTime += _initParams.UpdateDataPeriod;
        }

        public Task DisconnectAsync()
        {
            IsConnected = false;
            _workerController.CloseWorker(_worker);
            return Task.Delay(_initParams.DisconnectDelay);
        }

        public Task ExecuteCommandAsync(BedControlCommand command)
        {
            return Task.Delay(_initParams.DefaultDelay);
        }

        public async Task<TimeSpan> GetCycleDurationAsync()
        {
            await Task.Delay(_initParams.DefaultDelay);

            return _cycleDuration;
        }


        public async Task<short> GetCyclesCountAsync()
        {
            await Task.Delay(_initParams.DefaultDelay);
            return _initParams.CyclesCount;
        }

        public async Task<short> GetCurrentCycleNumberAsync()
        {
            await Task.Delay(_initParams.DefaultDelay);
            return (short)(Math.Round((double)_elapsedTime.Ticks / _cycleDuration.Ticks)+1);
        }

        public async Task<short> GetIterationsCountAsync()
        {
            await Task.Delay(_initParams.DefaultDelay);
            return _iterationsCount;
        }


        public async Task<short> GetCurrentIterationAsync()
        {
            await Task.Delay(_initParams.DefaultDelay);
            return GetCurrentIteration();
        }

        private short GetCurrentIteration()
        {
            var cycleElapsedTime = GetCycleElapsedTime();
            var result = cycleElapsedTime.Ticks * _iterationsCount / _cycleDuration.Ticks;
            return (short)(result + 1);
        }

        private TimeSpan GetCycleElapsedTime()
        {
            return TimeSpan.FromTicks((long)Math.Round((double)_elapsedTime.Ticks % _cycleDuration.Ticks));
        }

        public async Task<short> GetNextIterationNumberForPressureMeasuringAsync()
        {
            await Task.Delay(_initParams.DefaultDelay);
            var currentIteation = GetCurrentIteration();
            var temp = _checkPointIterationNumber.FirstOrDefault(x => x >= currentIteation);
            return temp == default(short)
                ? _iterationsCount
                : temp;
        }

        public async Task<short> GetNextIterationNumberForCommonParamsMeasuringAsync()
        {
            await Task.Delay(_initParams.DefaultDelay);
            var currentIteation = GetCurrentIteration();
            var temp = _checkPointIterationNumber.FirstOrDefault(x => x >= currentIteation);
            return temp == default(short)
                ? _iterationsCount
                : temp;
        }

        public async Task<short> GetNextIterationNumberForEcgMeasuringAsync()
        {
            await Task.Delay(_initParams.DefaultDelay);
            var currentIteation = GetCurrentIteration();
            var temp = _checkPointIterationNumber.FirstOrDefault(x => x >= currentIteation);
            return temp == default(short)
                ? _iterationsCount
                : temp;
        }

        public async Task<TimeSpan> GetRemainingTimeAsync()
        {
            await Task.Delay(_initParams.DefaultDelay);
            return _sessionDuration - _elapsedTime;
        }

        public async Task<TimeSpan> GetElapsedTimeAsync()
        {
            await Task.Delay(_initParams.DefaultDelay);
            return _elapsedTime;
        }

        public Task<StartFlag> GetStartFlagAsync()
        {
            return Task.FromResult(StartFlag.Default);
        }

        public Task<ReverseFlag> GetReverseFlagAsync()
        {
            return Task.FromResult(ReverseFlag.NotReversed);
        }

        public async Task<float> GetAngleXAsync()
        {
            await Task.Delay(_initParams.DefaultDelay);
            var cycleElapsedTime = GetCycleElapsedTime();
            if (cycleElapsedTime.Ticks >= _cycleDuration.Ticks / 2)
            {

                return (float) (_initParams.MaxAngleX * (_cycleDuration.Ticks - cycleElapsedTime.Ticks) /
                                _cycleDuration.Ticks);
            }

            return (float) (_initParams.MaxAngleX * cycleElapsedTime.Ticks / _cycleDuration.Ticks);
        }
    }

    public class FakeDeviceInitParams : IBedControllerInitParams
    {
        public double MaxAngleX { get; }
        public short CyclesCount { get; }
        public float MovementFrequency { get; }
        public TimeSpan UpdateDataPeriod { get; }
        public TimeSpan Timeout { get; }

        public TimeSpan ConenctDelay { get; set; }

        public TimeSpan DisconnectDelay { get; set; }

        public TimeSpan DefaultDelay { get; set; }

        public TimeSpan CycleWithMaxAngleDuration { get; set; }
       
    }
}