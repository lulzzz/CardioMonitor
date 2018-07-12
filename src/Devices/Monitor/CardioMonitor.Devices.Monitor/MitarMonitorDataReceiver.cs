using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CardioMonitor.Devices.Monitor.Infrastructure;

namespace CardioMonitor.Devices.Monitor
{
    public class MitarMonitorDataReceiver : IDisposable
    {
        #region fields

        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly NetworkStream _stream;
        private bool _isNeedDataRead;
        private short _heartRate;
        private short _repsirationRate;
        private short _spo2;
        private short _systolicArterialPressure;
        private short _diastolicArterialPressure;
        private short _averageArterialPressure;
        private PumpingStatus _status;

        #endregion
        
        public MitarMonitorDataReceiver(NetworkStream stream)
        {
            _stream = stream;
            _semaphoreSlim = new SemaphoreSlim(1, 1);
        }
        public void Start()
        {
            _isNeedDataRead = true;
            Task.Factory.StartNew(GetDataFromStream);
        }

        public void Stop()
        {
            _isNeedDataRead = false;
        }

        public Task<PatientCommonParams> GetCommonParams()
        {
            return Task.FromResult(new PatientCommonParams(_heartRate, _repsirationRate, _spo2));
        }

        public Task<PatientPressureParams> GetPressureParams()
        {
            return Task.FromResult(new PatientPressureParams(_systolicArterialPressure,_diastolicArterialPressure,_averageArterialPressure));
        }

        public Task<PumpingStatus> GetPumpingStatus()
        {
            return Task.FromResult(_status);
        }

        private async void GetDataFromStream()
        {
            await _semaphoreSlim
                .WaitAsync()
                .ConfigureAwait(false);
            try
            {
                while (_isNeedDataRead)
                {
                    if (_stream == null) continue;
                    
                    var buffer = new byte[64];
                    var buffSize = await _stream
                        .ReadAsync(buffer, 0, buffer.Length)
                        .ConfigureAwait(false);
                    if (buffSize > 0)
                    {
                        Parse(buffer);
                    }
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private void Parse(byte[] packet)
        {
            if (packet[0] >> 4 != 0xE) return;
            
            var forcrc = new byte[63];
            Array.ConstrainedCopy(packet, 0, forcrc, 0, 63);
            if (packet[63] != Crc8Calculator.GetCRC8(forcrc)) return;
            
            //все правильно - это пакет

            var a = packet[2] >> 4;
            var b = packet[4] >> 4;
            var idx = a + (b << 4);

            switch (idx)
            {
                case 10:
                {
                    _repsirationRate = GetIdxParamData(packet);
                    break;
                }
                case 12:
                {
                    _heartRate = GetIdxParamData(packet);
                    break;
                }
                case 13:
                {
                    _spo2 = GetIdxParamData(packet);
                    break;
                }
                case 17:
                {
                    _systolicArterialPressure = GetIdxParamData(packet);
                    break;
                }
                case 18:
                {
                    _diastolicArterialPressure = GetIdxParamData(packet);
                    break;
                }
                case 19:
                {
                    _averageArterialPressure = GetIdxParamData(packet);
                    break;
                }
                case 22:
                {
                    StatusParseTest(packet);
                    break;
                }
            }


        }

        private short GetIdxParamData(IReadOnlyList<byte> packet)
        {
            var valueLowFirst = packet[6] >> 4;
            var valueHighFirst = packet[8] >> 4;
            var valueLowSecond = packet[10] >> 4;
            var valueHighSecond = packet[12] >> 4;
            return  (short)(valueLowFirst + (valueHighFirst << 4) + (valueLowSecond << 8) + (valueHighSecond << 12));
        }

        private void StatusParseTest(IReadOnlyList<byte> packet)
        {
            var valueLowFirst = packet[6] >> 4;
            var valueHighFirst = packet[8] >> 4;
            var valueLowSecond = packet[10] >> 4;
            var valueHighSecond = packet[12] >> 4;
            var low = (byte)(valueLowFirst + (valueHighFirst << 4));
            var high = (byte)(valueLowSecond + (valueHighSecond << 4));

            var isPumpingError = high != 128;
            var isPumpingInProgress = low != 0;

            if (isPumpingError)
            {
                _status = PumpingStatus.Error;
                return;
            }

            if (isPumpingInProgress)
            {
                _status = PumpingStatus.InProgress;
                return;
            }

            _status = PumpingStatus.Completed;
        }


        public void Dispose()
        {
            _semaphoreSlim?.Dispose();
            _stream?.Close();
            _stream?.Dispose();
        }
    }
}
