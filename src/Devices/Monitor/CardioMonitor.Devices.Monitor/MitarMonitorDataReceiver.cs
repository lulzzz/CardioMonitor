using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CardioMonitor.Devices.Monitor.Infrastructure;

namespace CardioMonitor.Devices.Monitor
{
    public class MitarMonitorDataReceiver
    {
        #region fields
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private NetworkStream _stream;
        private short _pressureModuleStatus;
        private bool _isNeedDataRead;
        private short _heartRate;
        private short _repsirationRate;
        private short _spo2;
        private short _systolicArterialPressure;
        private short _diastolicArterialPressure;
        private short _averageArterialPressure;
        private bool _isPumpingError;
        private bool _isPumpingInProgress;

        #endregion
        

        public MitarMonitorDataReceiver(NetworkStream stream)
        {
            _stream = stream;
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
            return Task.Factory.StartNew(() => new PatientCommonParams(_heartRate, _repsirationRate, _spo2));
        }

        public Task<PatientPressureParams> GetPressureParams()
        {
            return Task.Factory.StartNew(() => new PatientPressureParams(_systolicArterialPressure,_diastolicArterialPressure,_averageArterialPressure));
        }

        public Task<PumpingStatus> GetPumpingStatus()
        {
            return Task.Factory.StartNew(() => new PumpingStatus(_isPumpingError,_isPumpingInProgress));
        }

        private async void GetDataFromStream()
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                while (_isNeedDataRead)
                {
                    if (_stream != null)
                    {
                        byte[] buffer = new byte[64];
                        var buffSize = await _stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                        if (buffSize > 0)
                        {
                            Parse(buffer);
                        }
                    }
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private void Parse(byte[] packet)
        {
            if (packet[0] >> 4 == 0xE)
            {
                byte[] forcrc = new byte[63];
                Array.ConstrainedCopy(packet, 0, forcrc, 0, 63);
                if (packet[63] == Crc8Calculator.GetCRC8(forcrc))
                {
                    //все правильно - это пакет

                    int a = packet[2] >> 4;
                    int b = packet[4] >> 4;
                    int idx = a + (b << 4);

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
            }


        }

        private short GetIdxParamData(byte[] packet)
        {
            int valueLowFirst = packet[6] >> 4;
            int valueHighFirst = packet[8] >> 4;
            int valueLowSecond = packet[10] >> 4;
            int valueHighSecond = packet[12] >> 4;
            return  (short)(valueLowFirst + (valueHighFirst << 4) + (valueLowSecond << 8) + (valueHighSecond << 12));
        }

        private void StatusParseTest(byte[] packet)
        {
            int valueLowFirst = packet[6] >> 4;
            int valueHighFirst = packet[8] >> 4;
            int valueLowSecond = packet[10] >> 4;
            int valueHighSecond = packet[12] >> 4;
            byte low = (byte)(valueLowFirst + (valueHighFirst << 4));
            byte high = (byte)(valueLowSecond + (valueHighSecond << 4));

            _isPumpingError = high != 128;
            _isPumpingInProgress = low != 0;
        }
            
        
    }
}
