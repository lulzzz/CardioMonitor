using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure.Workers;
using JetBrains.Annotations;

namespace CardioMonitor.Devices.Monitor
{
    /// <summary>
    /// Контроллер взаимодействия с кардиомонитором МИТАР
    /// </summary>
    public class MitarMonitorController : IMonitorController
    {
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        #region fields

        private const int IsRequestedValue = 1;
        
        //todo оставил, чтобы пока помнить адресс
//        private readonly int localUdpPort = 30304;
//        private IPAddress remoteMonitorIpAddress;
//        private readonly int remoteMonitorTcpPort = 9761;

        private NetworkStream _stream;
        private TcpClient _tcpClient;

        private MitarMonitorDataReceiver mitar;

        /// <remarks>
        /// NotNull помечено специально, чтобы анализатор не ругался. В каждом методе должны быть провеки методом <see cref="AssertInitParams"/>
        /// </remarks>
        [NotNull] private MitarMonitorControlerConfig _initParams;

        [NotNull] private readonly IWorkerController _workerController;

        [CanBeNull] private Worker _syncWorker;

        private PatientCommonParams _lastCommonParams;
        private PatientPressureParams _lastPressureParams;
        private PatientEcgParams _lastEcgParams;

        private PumpingStatus _pumpingStatus;

        private readonly ManualResetEventSlim _commonParamsReady;
        private readonly ManualResetEventSlim _pressureParamsReady;
        private readonly ManualResetEventSlim _ecgParamsReady;
        private readonly AutoResetEvent _pumpingReady;

        // используется для блокировки с помощью lock - чтобы в один момент времени выполнялся только один метод
        private readonly object _commonParamsRequestLockObject;
        private readonly object _pressureParamsRequestLockObject;
        private readonly object _ecgRequestLockObject;
        private readonly object _pumpingLockObject;

        private int _isCommonParamsRequested;
        private int _isPressureParamsRequested;
        private int _isEcgParamsRequested;

        private readonly ConcurrentQueue<short> _ecgValues;

        private DateTime _startedEcgCollectingTime;
        private TimeSpan _ecgCollectionDuration;

        private bool _isPumpStarted;
        private bool _isPumpStartPumping;
        
        [NotNull]
        private readonly ConcurrentQueue<Exception> _lastExceptions;
        #endregion

        // ReSharper disable once NotNullMemberIsNotInitialized
        public MitarMonitorController([NotNull] IWorkerController workerController)
        {
            _workerController = workerController ?? throw new ArgumentNullException(nameof(workerController));

            IsConnected = false;

            _commonParamsRequestLockObject = new object();
            _pressureParamsRequestLockObject = new object();
            _ecgRequestLockObject = new object();
            _pumpingLockObject = new object();
            
            _isCommonParamsRequested = 0;
            _isPressureParamsRequested = 0;
            _isEcgParamsRequested = 0;

            _commonParamsReady = new ManualResetEventSlim(false);
            _pressureParamsReady = new ManualResetEventSlim(false);
            _ecgParamsReady = new ManualResetEventSlim(false);
            _pumpingReady = new AutoResetEvent(false);
            _pumpingStatus = new PumpingStatus(false,false);
            
            _ecgValues = new ConcurrentQueue<short>();
            _lastExceptions = new ConcurrentQueue<Exception>();
        }

        #region Управление контроллером

        /// <inheritdoc />
        public bool IsConnected { get; private set; }


        public void Init(IMonitorControllerConfig initParams)
        {
            if (initParams == null) throw new ArgumentNullException(nameof(initParams));

            var mitarInitParams = initParams as MitarMonitorControlerConfig;
            _initParams = mitarInitParams ??
                          throw new ArgumentException(
                              $"{nameof(initParams)} должен быть типа {typeof(MitarMonitorControlerConfig)}");
        }

        public async Task ConnectAsync()
        {
            AssertInitParams();

            if (IsConnected) throw new InvalidOperationException($"{GetType().Name} уже подключен к устройству");

            try
            {
                // очистим перед подключением все накопленные ошибки
                while (_lastExceptions.TryDequeue(out var _))
                {
                }

                var monitorIpAddress = await DiscoverMonitorIpAddressAsync()
                    .ConfigureAwait(false);

                _tcpClient = new TcpClient();
                
                await _tcpClient.ConnectAsync(monitorIpAddress, _initParams.MonitorTcpPort)
                    .ConfigureAwait(false);
                _stream = _tcpClient.GetStream();
                mitar = new MitarMonitorDataReceiver(_stream);
                mitar.Start();
                // ReSharper disable once HeapView.CanAvoidClosure
                _syncWorker = _workerController.StartWorker(_initParams.UpdateDataPeriod, async () =>
                {
                    try
                    {
                        //todo сюды бы токен отмены
                        await UpdateDataAsync()
                            .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        IsConnected = false;
                        _workerController.CloseWorker(_syncWorker);
                        _lastExceptions.Enqueue(ex);
                    }

                });
                IsConnected = true;
            }
            catch (SocketException e)
            {
                IsConnected = false;
                throw new DeviceConnectionException("Ошибка подключения к кардиомонитору", e);
            }
            catch (ObjectDisposedException e)
            {
                IsConnected = false;
                throw new DeviceConnectionException("Ошибка подключения к кардиомонитору", e);
            }
            catch (Exception e)
            {
                IsConnected = false;
                throw new DeviceProcessingException("Ошибка подключения к кардиомонитору", e);
            }
        }

        private void AssertInitParams()
        {
            if (_initParams == null)
                throw new InvalidOperationException(
                    $"Контроллер не инициализирован. Необходимо сначала вызвать метод {nameof(Init)}");
        }

        private async Task<IPAddress> DiscoverMonitorIpAddressAsync()
        {
            var localUdpIp = new IPEndPoint(IPAddress.Any, _initParams.MonitorBroadcastUdpPort);
            using (var udpClient = new UdpClient(localUdpIp))
            {
                var response = await udpClient.ReceiveAsync()
                    .ConfigureAwait(false);

                return response.RemoteEndPoint.Address;
            }
        }

        private async Task UpdateDataAsync()
        {
            var isCommonParamsRequested = _isCommonParamsRequested == IsRequestedValue;
            var isPressureParamsRequested = _isPressureParamsRequested == IsRequestedValue;
            var isEcgParamsRequested = _isEcgParamsRequested == IsRequestedValue;

            if (!isCommonParamsRequested && !isPressureParamsRequested && !isEcgParamsRequested && !_isPumpStarted) return;
            AssertConnection();

            await semaphoreSlim.WaitAsync();

            short[] ecgValue =  new short[2];
            try
            {
                //MitarMonitorDataParser monitorDataParser = new MitarMonitorDataParser();
                //const int messageSize = 6400;
                //byte[] message = new byte[messageSize];
                //int i = 0;
                //while (i < messageSize)
                //{
                //    byte[] text = new byte[1024];
                //    var buffSize = await _stream.ReadAsync(text, 0, text.Length).ConfigureAwait(false);
                //    if (i + buffSize < messageSize)
                //    {
                //        Array.ConstrainedCopy(text, 0, message, i, buffSize);

                //    }

                //    i += buffSize;
                //    //var data = GetECGValue(text);
                //    //ECGData[i] = data[0];
                //    //ECGData[i + 1] = data[1];
                //    //i+=2;
               // }

                
                var commonParams = await mitar.GetCommonParams();
                var pressureParams = await mitar.GetPressureParams();
                _pumpingStatus = await mitar.GetPumpingStatus();
                //var pumpingResult = await mitar.GetPumpingStatus();

                //todo вот тут как-то получить данные и скастовать их к нужному виду
                /*await _stream.ReadAsync(message, 0, messageSize)
                    .ConfigureAwait(false);*/
                //commonParams = monitorDataParser.GetPatientCommonParams(message);
                if (_isPumpStarted)
                {
                    if (!_isPumpStartPumping && _pumpingStatus.IsPumpingInProgress)
                    {
                        _isPumpStartPumping = true;
                    }
                    if ( _isPumpStartPumping &&( _pumpingStatus.IsPumpingError || !_pumpingStatus.IsPumpingInProgress))
                    {
                        _isPumpStartPumping = false;
                        _isPumpStarted = false;
                        _pumpingReady.Set();
                    }
                }


                if (isCommonParamsRequested)
                {
                    _lastCommonParams = commonParams;
                    _commonParamsReady.Set();
                }

                if (isPressureParamsRequested)
                {
                    _lastPressureParams = pressureParams;
                    _pressureParamsReady.Set();
                }

                if (isEcgParamsRequested)
                {
                    _ecgValues.Enqueue(ecgValue[0]);
                    _ecgValues.Enqueue(ecgValue[1]);
                    var currentTime = DateTime.UtcNow;

                    if (currentTime > _startedEcgCollectingTime &&
                        currentTime - _startedEcgCollectingTime >= _ecgCollectionDuration)
                    {
                        _lastEcgParams = new PatientEcgParams(_ecgValues.ToArray());
                        while (_ecgValues.TryDequeue(out var _))
                        {
                        }

                        _ecgParamsReady.Set();
                    }
                }
            }
            catch (Exception)
            {
                // чтобы не было deadlock'ов
                if (isCommonParamsRequested)
                {
                    _commonParamsReady.Set();
                }

                if (isPressureParamsRequested)
                {
                    _commonParamsReady.Set();
                }

                if (isEcgParamsRequested)
                {
                    _ecgParamsReady.Set();
                }

                throw;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }


        public Task DisconnectAsync()
        {
            try
            {
                mitar.Stop();
                Thread.Sleep(100);
                _workerController.CloseWorker(_syncWorker);
                 
                 mitar = null;
                _stream?.Dispose();
                _stream = null;
                _tcpClient?.Dispose();
                _tcpClient = null;
                return Task.CompletedTask;
            }
            catch (SocketException e)
            {
                IsConnected = false;
                throw new DeviceConnectionException("Ошибка отключения от кардиомонитора", e);
            }
            catch (ObjectDisposedException e)
            {
                IsConnected = false;
                throw new DeviceConnectionException("Ошибка отключения от кардиомонитора", e);
            }
            catch (Exception e)
            {
                IsConnected = false;
                throw new DeviceProcessingException("Ошибка отключения от кардиомонитора", e);
            }
        }


        public void Dispose()
        {
            _workerController.CloseWorker(_syncWorker);
            _stream?.Dispose();
            _tcpClient?.Dispose();
        }

        #endregion

        #region Получение данных

        /// <summary>
        /// накачка давления
        /// </summary>
        /// <returns></returns>
        public Task PumpCuffAsync()
        {
            AssertConnection();
            lock (_pumpingLockObject)
            {
                return Task.Factory.StartNew(() =>
                {
                    try
                    {


                        byte[] sendMessage = new byte[25]
                        {
                            0x70, 0x10, 0x50, 0x50, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x22
                        };
                        _stream.WriteAsync(sendMessage, 0, sendMessage.Length);
                        
                        _isPumpStarted = true;
                        _pumpingReady.WaitOne(new TimeSpan(0, 1,
                            0)); //todo по таймауту по идее должен кидаться эксепшн (подумать над необходимостью)
                        _isPumpStarted = false;
                        if (_pumpingStatus.IsPumpingError)
                        {
                            throw new ArgumentException(
                                "Ошибка накачки давления "); //todo не знаю какого типа эксепшен вызывать
                        }
                    }
                    catch (Exception)
                    {
                       // throw new DeviceConnectionException("Ошибка накачки давления");
                    }
                    finally
                    {
                        //RiseExceptions();
                    }
                });

            
            }
        }
        
        private void RiseExceptions()
        {
            var exceptions = new List<Exception>(0);
            while (_lastExceptions.TryPeek(out var temp))
            {
                exceptions.Add(temp);
            }
            if (exceptions.Count == 0) return;
            var hasConnectionExceptions = exceptions.Any(x =>
                x.GetType() == typeof(SocketException) || x.GetType() == typeof(ObjectDisposedException));
            var agregatedException = new AggregateException(exceptions);

            if (hasConnectionExceptions)
            {
                throw new DeviceConnectionException("Ошибка во взаимодействии с кардиомонитором", agregatedException);
            }
            throw new DeviceProcessingException("Ошибка обработки данных от кардиомонитора", agregatedException);
        }
        

        private void AssertConnection()
        {
            if (!IsConnected)
                throw new InvalidOperationException(
                    $"Соединение с монитором не уставнолено. Установите соединение с помощью метода {nameof(ConnectAsync)}");
        }

        public Task<PatientCommonParams> GetPatientCommonParamsAsync()
        {
            AssertConnection();
            lock (_commonParamsRequestLockObject)
            {
                return Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Interlocked.Increment(ref _isCommonParamsRequested);
                        _commonParamsReady.Wait();
                        _commonParamsReady.Reset();
                        return _lastCommonParams;
                    }
                    // тут могут быть только наши ошибки, сетевые генерятся в цикле
                    catch (Exception ex)
                    {
                        throw new DeviceProcessingException("Программная ошибка в получении параметров пациента с кардиомонитора", ex);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _isCommonParamsRequested);
                        RiseExceptions();
                    }
                });
            }
        }

        public Task<PatientPressureParams> GetPatientPressureParamsAsync()
        {
            AssertConnection();
            lock (_pressureParamsRequestLockObject)
            {
                return Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Interlocked.Increment(ref _isPressureParamsRequested);
                        _pressureParamsReady.Wait();
                        _pressureParamsReady.Reset();
                        return _lastPressureParams;
                    }
                    // тут могут быть только наши ошибки, сетевые генерятся в цикле
                    catch (Exception ex)
                    {
                        throw new DeviceProcessingException("Программная ошибка в получении параметров давления пациента с кардиомонитора", ex);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _isPressureParamsRequested);
                        RiseExceptions();
                    }
                });
            }
        }

        public Task<PatientEcgParams> GetPatientEcgParamsAsync(TimeSpan duration)
        {
            AssertConnection();
            lock (_ecgRequestLockObject)
            {
                return Task.Factory.StartNew(() =>
                {
                    try
                    {
                        _startedEcgCollectingTime = DateTime.UtcNow;
                        _ecgCollectionDuration = duration;
                        Interlocked.Increment(ref _isEcgParamsRequested);
                        _ecgParamsReady.Wait();
                        _ecgParamsReady.Reset();
                        return _lastEcgParams;
                    }
                    // тут могут быть только наши ошибки, сетевые генерятся в цикле
                    catch (Exception ex)
                    {
                        throw new DeviceProcessingException("Программная ошибка в получении параметров ЭКГ с кардиомонитора", ex);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _isEcgParamsRequested);
                        RiseExceptions();
                    }
                });
            }
        }

        #endregion

        public Guid DeviceId => MitarMonitorDeviceId.DeviceId;
        public Guid DeviceTypeId => MonitorDeviceTypeId.DeviceTypeId;
    }
}
