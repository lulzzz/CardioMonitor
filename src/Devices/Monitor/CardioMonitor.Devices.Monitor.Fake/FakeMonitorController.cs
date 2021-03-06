﻿using System;
using System.Threading.Tasks;
using CardioMonitor.Devices.Monitor.Infrastructure;

namespace CardioMonitor.Devices.Monitor.Fake
{
    public class FakeMonitorController : IMonitorController
    {
        private FakeCardioMonitorConfig _config;
        private Random _randomizer;

        public FakeMonitorController()
        {
            _randomizer = new Random();
        }

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
            if (_config == null) return Task.CompletedTask;
            return Task.Delay(_config.DefaultDelay);
        }

        public Task DisconnectAsync()
        {
            if (_config == null) return Task.CompletedTask;
            
            return Task.Delay(_config.DefaultDelay);
        }

        public Task PumpCuffAsync()
        {
            if (_config == null) return Task.CompletedTask;
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
}