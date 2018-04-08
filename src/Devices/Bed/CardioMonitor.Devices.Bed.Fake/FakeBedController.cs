using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CardioMonitor.Devices.Bed.Infrastructure;

namespace CardioMonitor.Devices.Bed.Fake
{
    public class FakeBedController : IBedController
    {
        private FakeDeviceInitParams _initParams;

        public void Dispose()
        {
        }

        public event EventHandler OnPauseFromDeviceRequested;
        public event EventHandler OnResumeFromDeviceRequested;
        public event EventHandler OnEmeregencyStopFromDeviceRequested;
        public event EventHandler OnReverseFromDeviceRequested;

        public bool IsConnected { get; private set; }
        public void Init(IBedControllerInitParams initParams)
        {
            if (!(initParams is FakeDeviceInitParams localParms)) throw new ArgumentException();

            _initParams = localParms;
        }

        public Task ConnectAsync()
        {
            IsConnected = true;
            return Task.Delay(TimeSpan.FromSeconds(1));
        }

        public Task DisconnectAsync()
        {
            IsConnected = false;
            return Task.Delay(TimeSpan.FromSeconds(1));
        }

        public Task ExecuteCommandAsync(BedControlCommand command)
        {
            return Task.Delay(TimeSpan.FromSeconds(5));
        }

        public Task<TimeSpan> GetCycleDurationAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<short> GetCyclesCountAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            return _initParams.CyclesCount;
        }

        public Task<short> GetCurrentCycleNumberAsync()
        {
            throw new NotImplementedException();
        }

        public Task<short> GetIterationsCountAsync()
        {
            throw new NotImplementedException();
        }

        public Task<short> GetCurrentIterationAsync()
        {
            throw new NotImplementedException();
        }

        public Task<short> GetNextIterationNumberForPressureMeasuringAsync()
        {
            throw new NotImplementedException();
        }

        public Task<short> GetNextIterationNumberForCommonParamsMeasuringAsync()
        {
            throw new NotImplementedException();
        }

        public Task<short> GetNextIterationNumberForEcgMeasuringAsync()
        {
            throw new NotImplementedException();
        }

        public Task<TimeSpan> GetRemainingTimeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<TimeSpan> GetElapsedTimeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<StartFlag> GetStartFlagAsync()
        {
            return Task.FromResult(StartFlag.Default);
        }

        public Task<ReverseFlag> GetReverseFlagAsync()
        {
            return Task.FromResult(ReverseFlag.NotReversed);
        }

        public Task<float> GetAngleXAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class FakeDeviceInitParams : IBedControllerInitParams
    {
        public double MaxAngleX { get; }
        public short CyclesCount { get; }
        public float MovementFrequency { get; }
        public TimeSpan UpdateDataPeriod { get; }
        public TimeSpan Timeout { get; }
    }
}