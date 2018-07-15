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
    //todo использовать параметры таймаута из настроек
    public class MitarMonitorController : IMonitorController
    {
        #region Fields

        private const int IsRequestedValue = 1;
        
        private readonly SemaphoreSlim _updateDataSyncSemaphore;
        private NetworkStream _stream;
        private TcpClient _tcpClient;

        private MitarMonitorDataReceiver _mitar;

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

        private readonly Nito.AsyncEx.AsyncManualResetEvent _commonParamsReady;
        private readonly Nito.AsyncEx.AsyncManualResetEvent _pressureParamsReady;
        private readonly Nito.AsyncEx.AsyncManualResetEvent _ecgParamsReady;
        private readonly Nito.AsyncEx.AsyncManualResetEvent _pumpingReady;
        
        private int _isCommonParamsRequested;
        private int _isPressureParamsRequested;
        private int _isEcgParamsRequested;

        private readonly ConcurrentQueue<short> _ecgValues;

        private DateTime _startedEcgCollectingTime;
        private TimeSpan _ecgCollectionDuration;

        private bool _isPumpingRequested;
        private bool _isPumpingStarted;
        
        [NotNull]
        private readonly ConcurrentQueue<Exception> _lastExceptions;
        #endregion

        // ReSharper disable once NotNullMemberIsNotInitialized
        public MitarMonitorController([NotNull] IWorkerController workerController)
        {
            _workerController = workerController ?? throw new ArgumentNullException(nameof(workerController));

            IsConnected = false;
            
            _isCommonParamsRequested = 0;
            _isPressureParamsRequested = 0;
            _isEcgParamsRequested = 0;

            _commonParamsReady = new Nito.AsyncEx.AsyncManualResetEvent(false);
            _pressureParamsReady = new Nito.AsyncEx.AsyncManualResetEvent(false);
            _ecgParamsReady = new Nito.AsyncEx.AsyncManualResetEvent(false);
            _pumpingReady = new Nito.AsyncEx.AsyncManualResetEvent(false);
            _pumpingStatus = PumpingStatus.Completed;
            _updateDataSyncSemaphore = new SemaphoreSlim(1, 1);


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
                while (_lastExceptions.TryDequeue(out _))
                {
                }

                var monitorIpAddress = await DiscoverMonitorIpAddressAsync()
                    .ConfigureAwait(false);

                _tcpClient = new TcpClient();
                
                await _tcpClient.ConnectAsync(monitorIpAddress, _initParams.MonitorTcpPort)
                    .ConfigureAwait(false);
                _stream = _tcpClient.GetStream();
                _mitar = new MitarMonitorDataReceiver(_stream);
                _mitar.Start();
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
                var response = await udpClient
                    .ReceiveAsync()
                    .ConfigureAwait(false);

                return response.RemoteEndPoint.Address;
            }
        }

        private async Task UpdateDataAsync()
        {
            var isCommonParamsRequested = _isCommonParamsRequested == IsRequestedValue;
            var isPressureParamsRequested = _isPressureParamsRequested == IsRequestedValue;
            var isEcgParamsRequested = _isEcgParamsRequested == IsRequestedValue;

            if (!isCommonParamsRequested && !isPressureParamsRequested && !isEcgParamsRequested && !_isPumpingRequested) return;
            AssertConnection();

            await _updateDataSyncSemaphore
                .WaitAsync()
                .ConfigureAwait(false);

            var ecgValue =  new short[2];
            try
            {
                var commonParams = await _mitar
                    .GetCommonParams()
                    .ConfigureAwait(false);
                var pressureParams = await _mitar
                    .GetPressureParams()
                    .ConfigureAwait(false);
                _pumpingStatus = await _mitar
                    .GetPumpingStatus()
                    .ConfigureAwait(false);

                if (_isPumpingRequested)
                {
                    if (!_isPumpingStarted && _pumpingStatus == PumpingStatus.InProgress)
                    {
                        _isPumpingStarted = true;
                    }
                    if ( _isPumpingStarted 
                         && ( _pumpingStatus != PumpingStatus.InProgress))
                    {
                        _isPumpingStarted = false;
                        _isPumpingRequested = false;
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
                        while (_ecgValues.TryDequeue(out _))
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
                _updateDataSyncSemaphore.Release();
            }
        }

        public Task DisconnectAsync()
        {
            try
            {
                _mitar.Stop();
                _workerController.CloseWorker(_syncWorker);
                Thread.Sleep(100);
                 
                 _mitar = null;
                _stream?.Close();
                _stream?.Dispose();
                _stream = null;
                _tcpClient?.Close();
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
            _mitar?.Stop();
            _mitar?.Dispose();
            _workerController.CloseWorker(_syncWorker);
            _stream?.Close();
            _stream?.Dispose();
            _tcpClient?.Close();
            _tcpClient?.Dispose();
        }

        #endregion

        #region Получение данных

        /// <summary>
        /// накачка давления
        /// </summary>
        /// <returns></returns>
        public async Task PumpCuffAsync()
        {
            AssertConnection();
            try
            {
                byte[] sendMessage =
                {
                    0x70, 0x10, 0x50, 0x50, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x22
                };
                await _stream
                    .WriteAsync(sendMessage, 0, sendMessage.Length)
                    .ConfigureAwait(false);
                _isPumpingRequested = true;
                _pumpingReady
                    .WaitAsync()
                    .ConfigureAwait(false);
                _pumpingReady.Reset();
                // ждем пока накачается
                await Task
                    .Delay(TimeSpan.FromSeconds(10))
                    .ConfigureAwait(false);
                _isPumpingRequested = false;
                if (_pumpingStatus == PumpingStatus.Error)
                {
                    throw new DeviceProcessingException(
                        "Ошибка накачки давления ");
                }
            }
            catch (DeviceProcessingException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DeviceProcessingException("Ошибка накачки давления", ex);
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

        public async Task<PatientCommonParams> GetPatientCommonParamsAsync()
        {
            AssertConnection();
            try
            {
                Interlocked.Increment(ref _isCommonParamsRequested);
                await _commonParamsReady
                    .WaitAsync()
                    .ConfigureAwait(false);
                _commonParamsReady.Reset();
                return _lastCommonParams;
            }
            // тут могут быть только наши ошибки, сетевые генерятся в цикле
            catch (Exception ex)
            {
                throw new DeviceProcessingException(
                    "Программная ошибка в получении параметров пациента с кардиомонитора", ex);
            }
            finally
            {
                Interlocked.Decrement(ref _isCommonParamsRequested);
                RiseExceptions();
            }
        }

        public async Task<PatientPressureParams> GetPatientPressureParamsAsync()
        {
            AssertConnection();
            try
            {
                Interlocked.Increment(ref _isPressureParamsRequested);
                await _pressureParamsReady
                    .WaitAsync()
                    .ConfigureAwait(false);
                _pressureParamsReady.Reset();
                return _lastPressureParams;
            }
            // тут могут быть только наши ошибки, сетевые генерятся в цикле
            catch (Exception ex)
            {
                throw new DeviceProcessingException(
                    "Программная ошибка в получении параметров давления пациента с кардиомонитора", ex);
            }
            finally
            {
                Interlocked.Decrement(ref _isPressureParamsRequested);
                RiseExceptions();
            }
        }

        public async Task<PatientEcgParams> GetPatientEcgParamsAsync(TimeSpan duration)
        {
            AssertConnection();
            try
            {
                _startedEcgCollectingTime = DateTime.UtcNow;
                _ecgCollectionDuration = duration;
                Interlocked.Increment(ref _isEcgParamsRequested);
                await _ecgParamsReady
                    .WaitAsync()
                    .ConfigureAwait(false);
                _ecgParamsReady.Reset();
                return _lastEcgParams;
            }
            // тут могут быть только наши ошибки, сетевые генерятся в цикле
            catch (Exception ex)
            {
                throw new DeviceProcessingException(
                    "Программная ошибка в получении параметров ЭКГ с кардиомонитора", ex);
            }
            finally
            {
                Interlocked.Decrement(ref _isEcgParamsRequested);
                RiseExceptions();
            }
        }

        #endregion

        public Guid DeviceId => MitarMonitorDeviceId.DeviceId;
        public Guid DeviceTypeId => MonitorDeviceTypeId.DeviceTypeId;
    }
}
