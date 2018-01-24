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
        private const int IsRequestedValue = 1;
        
        //todo оставил, чтобы пока помнить адресс
//        private readonly int localUdpPort = 30304;
//        private IPAddress remoteMonitorIpAddress;
//        private readonly int remoteMonitorTcpPort = 9761;

        private NetworkStream _stream;
        private TcpClient _tcpClient;

        /// <remarks>
        /// NotNull помечено специально, чтобы анализатор не ругался. В каждом методе должны быть провеки методом <see cref="AssertInitParams"/>
        /// </remarks>
        [NotNull] private MitarMonitorControlerInitParams _initParams;

        [NotNull] private readonly IWorkerController _workerController;

        [CanBeNull] private Worker _syncWorker;

        private PatientCommonParams _lastCommonParams;
        private PatientPressureParams _lastPressureParams;
        private PatientEcgParams _lastEcgParams;

        private readonly ManualResetEventSlim _commonParamsReady;
        private readonly ManualResetEventSlim _pressureParamsReady;
        private readonly ManualResetEventSlim _ecgParamsReady;

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
        
        [NotNull]
        private readonly ConcurrentQueue<Exception> _lastExceptions;

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
            
            _ecgValues = new ConcurrentQueue<short>();
            _lastExceptions = new ConcurrentQueue<Exception>();
        }

        #region Управление контроллером

        /// <inheritdoc />
        public bool IsConnected { get; private set; }


        public void Init(IMonitorControllerInitParams initParams)
        {
            if (initParams == null) throw new ArgumentNullException(nameof(initParams));

            var mitarInitParams = initParams as MitarMonitorControlerInitParams;
            _initParams = mitarInitParams ??
                          throw new ArgumentException(
                              $"{nameof(initParams)} должен быть типа {typeof(MitarMonitorControlerInitParams)}");
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

            if (!isCommonParamsRequested && !isPressureParamsRequested && !isEcgParamsRequested) return;
            AssertConnection();


            PatientPressureParams pressureParams = null;
            PatientCommonParams commonParams = null;

            short ecgValue = 0;
            try
            {
                byte[] message = null;
                const int MessageSize = 100;
                //todo вот тут как-то получить даныне и скастовать их к нужному виду
                await _stream.ReadAsync(message, 0, MessageSize)
                    .ConfigureAwait(false);
                if (isCommonParamsRequested)
                {
                    _lastCommonParams = commonParams;
                    _commonParamsReady.Set();
                }
                if (isPressureParamsRequested)
                {
                    _lastPressureParams = pressureParams;
                    _commonParamsReady.Set();
                }
                if (isEcgParamsRequested)
                {
                    _ecgValues.Enqueue(ecgValue);
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
        }


        public async Task DisconnectAsync()
        {
            AssertConnection();
            try
            {

                await Task.Yield();
                _stream.Dispose();
                _stream = null;
                _tcpClient.Dispose();
                _tcpClient = null;
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

        public Task PumpCuffAsync()
        {
            AssertConnection();
            lock (_pumpingLockObject)
            {
                throw new NotImplementedException();
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
    }
}
