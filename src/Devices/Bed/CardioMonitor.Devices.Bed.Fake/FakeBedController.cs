﻿using System;
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
        private FakeBedControllerConfig _config;
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

        /// <summary>
        /// Специальная задержка для более корректного определения повторения и итерации
        /// </summary>
        private readonly TimeSpan _magicDelay;

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
            _magicDelay = TimeSpan.FromMilliseconds(300);
        }

        public bool IsConnected { get; private set; }

        public void InitController(IBedControllerConfig config)
        {
            if (!(config is FakeBedControllerConfig localParms)) throw new ArgumentException();

            _config = localParms;
            Init();
        }

        public Task ConnectAsync()
        {
            IsConnected = true;
            _worker = _workerController.StartWorker(_config.UpdateDataPeriod, WorkMethod);
            
            return Task.Delay(_config.ConnectDelay);
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

            for (var i = 1; i <= _checkPointsCount; ++i)
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

        public Task PrepareDeviceForSessionAsync()
        {
            return Task.Delay(_config.ConnectDelay);
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
            return Math.Min((short)(Math.Round((double)(_elapsedTime.Ticks - _magicDelay.Ticks) / _cycleDuration.Ticks)+1), _config.CyclesCount);
        }

        public async Task<short> GetIterationsCountAsync()
        {
            await Task.Delay(_config.DefaultDelay);
            return _iterationsCount;
        }


        public async Task<short> GetCurrentIterationAsync()
        {
            await Task.Delay(_config.DefaultDelay);
            return Math.Min(GetCurrentIteration(), _iterationsCount);
        }

        private short GetCurrentIteration()
        {
            var cycleElapsedTime = GetCycleElapsedTime();
            var result = (cycleElapsedTime.Ticks -_magicDelay.Ticks) * _iterationsCount / _cycleDuration.Ticks;
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

        public Task<BedStatus> GetBedStatusAsync()
        {
            return Task.FromResult(BedStatus.Ready);
        }

        public Guid DeviceId => FakeInversionTableDeviceId.DeviceId;
        public Guid DeviceTypeId => InversionTableDeviceTypeId.DeviceTypeId;
    }
}