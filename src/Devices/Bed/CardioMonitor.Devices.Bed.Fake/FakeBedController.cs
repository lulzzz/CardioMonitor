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
        private FakeDeviceConfig _config;
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

        public void Init(IBedControllerConfig config)
        {
            if (!(config is FakeDeviceConfig localParms)) throw new ArgumentException();

            _config = localParms;
            Init();
        }

        public Task ConnectAsync()
        {
            IsConnected = true;
            _worker = _workerController.StartWorker(_config.UpdateDataPeriod, WorkMethod);
            
            return Task.Delay(_config.ConenctDelay);
        }

        private void Init()
        {
            _cycleDuration = GetCycleDuration();
            _sessionDuration = TimeSpan.FromTicks(_cycleDuration.Ticks * _config.CyclesCount);
            _iterationsCount = GetIterationsCount();

            _checkPointsCount = (short)Math.Round(CheckPointsCountOnMaxAngleX * _config.MaxAngleX / MaxAngleX);
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
            var ticks = (long)Math.Round(_config.CycleWithMaxAngleDuration.Ticks / MaxAngleX * _config.MaxAngleX);
            return TimeSpan.FromTicks(ticks);
        }

        private short GetIterationsCount()
        {
            return (short)Math.Round(IterationsCountOnMaxAngleX / MaxAngleX * _config.MaxAngleX);
        }

        private void WorkMethod()
        {
            _elapsedTime += _config.UpdateDataPeriod;
        }

        public Task DisconnectAsync()
        {
            IsConnected = false;
            _workerController.CloseWorker(_worker);
            return Task.Delay(_config.DisconnectDelay);
        }

        public Task ExecuteCommandAsync(BedControlCommand command)
        {
            return Task.Delay(_config.DefaultDelay);
        }

        public async Task<TimeSpan> GetCycleDurationAsync()
        {
            await Task.Delay(_config.DefaultDelay);

            return _cycleDuration;
        }


        public async Task<short> GetCyclesCountAsync()
        {
            await Task.Delay(_config.DefaultDelay);
            return _config.CyclesCount;
        }

        public async Task<short> GetCurrentCycleNumberAsync()
        {
            await Task.Delay(_config.DefaultDelay);
            return (short)(Math.Round((double)_elapsedTime.Ticks / _cycleDuration.Ticks)+1);
        }

        public async Task<short> GetIterationsCountAsync()
        {
            await Task.Delay(_config.DefaultDelay);
            return _iterationsCount;
        }


        public async Task<short> GetCurrentIterationAsync()
        {
            await Task.Delay(_config.DefaultDelay);
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
            await Task.Delay(_config.DefaultDelay);
            var currentIteation = GetCurrentIteration();
            var temp = _checkPointIterationNumber.FirstOrDefault(x => x >= currentIteation);
            return temp == default(short)
                ? _iterationsCount
                : temp;
        }

        public async Task<short> GetNextIterationNumberForCommonParamsMeasuringAsync()
        {
            await Task.Delay(_config.DefaultDelay);
            var currentIteation = GetCurrentIteration();
            var temp = _checkPointIterationNumber.FirstOrDefault(x => x >= currentIteation);
            return temp == default(short)
                ? _iterationsCount
                : temp;
        }

        public async Task<short> GetNextIterationNumberForEcgMeasuringAsync()
        {
            await Task.Delay(_config.DefaultDelay);
            var currentIteation = GetCurrentIteration();
            var temp = _checkPointIterationNumber.FirstOrDefault(x => x >= currentIteation);
            return temp == default(short)
                ? _iterationsCount
                : temp;
        }

        public async Task<TimeSpan> GetRemainingTimeAsync()
        {
            await Task.Delay(_config.DefaultDelay);
            return _sessionDuration - _elapsedTime;
        }

        public async Task<TimeSpan> GetElapsedTimeAsync()
        {
            await Task.Delay(_config.DefaultDelay);
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
            await Task.Delay(_config.DefaultDelay);
            var cycleElapsedTime = GetCycleElapsedTime();
            if (cycleElapsedTime.Ticks >= _cycleDuration.Ticks / 2)
            {

                return (float) (_config.MaxAngleX * (_cycleDuration.Ticks - cycleElapsedTime.Ticks) /
                                _cycleDuration.Ticks);
            }

            return (float) (_config.MaxAngleX * cycleElapsedTime.Ticks / _cycleDuration.Ticks);
        }

        public Guid DeviceId => FakeInversionTableDeviceId.DeviceId;
        public Guid DeviceTypeId => InversionTableDeviceTypeId.DeviceTypeId;
    }

    public class FakeDeviceConfig : IBedControllerConfig
    {
        public float MaxAngleX { get; }
        public short CyclesCount { get; }
        public float MovementFrequency { get; }
        public TimeSpan UpdateDataPeriod { get; }
        public TimeSpan Timeout { get; }

        public TimeSpan? DeviceReconnectionTimeout { get; }

        public TimeSpan ConenctDelay { get; set; }

        public TimeSpan DisconnectDelay { get; set; }

        public TimeSpan DefaultDelay { get; set; }

        public TimeSpan CycleWithMaxAngleDuration { get; set; }
       
    }
}