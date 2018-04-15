using System;
using System.Threading.Tasks;
using CardioMonitor.Devices.Monitor.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CardioMonitor.Devices.Monitor.Fake
{
    public class FakeMonitorController : IMonitorController
    {
        private FakeCardioMonitorConfig _config;
        private Random _randomizer;

        public void Dispose()
        {
        }

        public bool IsConnected { get; }
        public void Init(IMonitorControllerConfig config)
        {
            if (!(config is FakeCardioMonitorConfig temp)) throw new ArgumentException();

            _config = temp;
        }

        public Task ConnectAsync()
        {
            return Task.Delay(_config.DefaultDelay);
        }

        public Task DisconnectAsync()
        {
            return Task.Delay(_config.DefaultDelay);
        }

        public Task PumpCuffAsync()
        {
            return Task.Delay(_config.PumpingDelay);
        }

        public async Task<PatientCommonParams> GetPatientCommonParamsAsync()
        {
            await Task.Delay(_config.DefaultDelay);
            return new PatientCommonParams(
                (short)_randomizer.Next(50,120), 
                (short)_randomizer.Next(0, 100), 
                (short)_randomizer.Next(0, 100));
        }

        public async Task<PatientPressureParams> GetPatientPressureParamsAsync()
        {
            await Task.Delay(_config.DefaultDelay);
            return new PatientPressureParams(
                (short)_randomizer.Next(50, 180),
                (short)_randomizer.Next(50, 180),
                (short)_randomizer.Next(50, 180));
        }

        public async Task<PatientEcgParams> GetPatientEcgParamsAsync(TimeSpan duration)
        {
            await Task.Delay(_config.DefaultDelay);
            return new PatientEcgParams(new short[0]);
        }

        public Guid DeviceId => FakeMonitorDeviceId.DeviceId;
        public Guid DeviceTypeId => MonitorDeviceTypeId.DeviceTypeId;
    }

    public class FakeCardioMonitorConfig : IMonitorControllerConfig
    {
        public FakeCardioMonitorConfig(
            TimeSpan updateDataPeriod, 
            TimeSpan timeout, 
            TimeSpan defaultDelay, 
            TimeSpan pumpingDelay)
        {
            UpdateDataPeriod = updateDataPeriod;
            Timeout = timeout;
            DefaultDelay = defaultDelay;
            PumpingDelay = pumpingDelay;
        }

        [JsonProperty("UpdateDataPeriod")]
        public TimeSpan UpdateDataPeriod { get; }

        [JsonProperty("Timeout")]
        public TimeSpan Timeout { get; }

        [JsonProperty("DefaultDelay")]
        public TimeSpan DefaultDelay { get; }

        [JsonProperty("PumpingDelay")]
        public TimeSpan PumpingDelay { get;  }
    }
}