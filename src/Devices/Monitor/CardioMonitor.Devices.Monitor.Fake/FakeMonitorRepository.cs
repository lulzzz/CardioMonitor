using System;
using System.Threading.Tasks;
using CardioMonitor.Devices.Monitor.Infrastructure;

namespace CardioMonitor.Devices.Monitor.Fake
{
    public class FakeMonitorRepository : IMonitorController
    {
        private FakeCardioMonitorInitParams _initParams;
        private Random _randomizer;

        public void Dispose()
        {
        }

        public bool IsConnected { get; }
        public void Init(IMonitorControllerInitParams initParams)
        {
            if (!(initParams is FakeCardioMonitorInitParams temp)) throw new ArgumentException();

            _initParams = temp;
        }

        public Task ConnectAsync()
        {
            return Task.Delay(_initParams.DefaultDelay);
        }

        public Task DisconnectAsync()
        {
            return Task.Delay(_initParams.DefaultDelay);
        }

        public Task PumpCuffAsync()
        {
            return Task.Delay(_initParams.PumpingDelay);
        }

        public async Task<PatientCommonParams> GetPatientCommonParamsAsync()
        {
            await Task.Delay(_initParams.DefaultDelay);
            return new PatientCommonParams(
                (short)_randomizer.Next(50,120), 
                (short)_randomizer.Next(0, 100), 
                (short)_randomizer.Next(0, 100));
        }

        public async Task<PatientPressureParams> GetPatientPressureParamsAsync()
        {
            await Task.Delay(_initParams.DefaultDelay);
            return new PatientPressureParams(
                (short)_randomizer.Next(50, 180),
                (short)_randomizer.Next(50, 180),
                (short)_randomizer.Next(50, 180));
        }

        public async Task<PatientEcgParams> GetPatientEcgParamsAsync(TimeSpan duration)
        {
            await Task.Delay(_initParams.DefaultDelay);
            return new PatientEcgParams(new short[0]);
        }

        public Guid DeviceId => FakeMonitorDeviceId.DeviceId;
        public Guid DeviceTypeId => MonitorDeviceTypeId.DeviceTypeId;
    }

    public class FakeCardioMonitorInitParams : IMonitorControllerInitParams
    {
        public TimeSpan UpdateDataPeriod { get; }
        public TimeSpan Timeout { get; }

        public TimeSpan DefaultDelay { get; set; }

        public TimeSpan PumpingDelay { get; set; }
    }
}