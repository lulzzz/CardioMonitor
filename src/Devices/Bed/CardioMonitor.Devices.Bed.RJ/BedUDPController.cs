using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Infrastructure;
using CardioMonitor.Infrastructure.Workers;
using JetBrains.Annotations;

namespace CardioMonitor.Devices.Bed.UDP
{
    /// <summary>
    /// Работа с кроватью по сети
    /// протокол передачи посылок - UDP
    /// </summary>
    public class BedUDPController : IBedController
    {
        private const int DeviceIsInitialising = 1;

        private const int DeviceIsNotInitialising = 0;
        
        //todo оставил, чтобы пока помнить адресс
        //private IPEndPoint _bedIpEndPoint = new IPEndPoint(IPAddress.Parse("192.168.56.3"), 7777);

        private IPEndPoint _bedIpEndPoint;
        
        
        /// <summary>
        /// Конечная точка, от которой была получена Udp дейтаграмма в ходе операции получения данных
        /// </summary>
        private IPEndPoint _remoteRecievedIpEndPoint;
        private readonly TimeSpan _bedInitStepDelay;
        
        /// <remarks>
        /// NotNull помечено специально, чтобы анализатор не ругался. В каждом методе должны быть провеки методом <see cref="AssertInitParams"/>
        /// </remarks>
        [NotNull] 
        private BedUdpControllerConfig _config;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// NotNull помечено специально, чтобы анализатор не ругался. В каждом методе должны быть провеки методом <see cref="AssertConnection"/>
        /// </remarks>
        [NotNull]
        private UdpClient _udpSendingClient;
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// NotNull помечено специально, чтобы анализатор не ругался. В каждом методе должны быть провеки методом <see cref="AssertConnection"/>
        /// </remarks>
        [NotNull]
        private UdpClient _udpReceivingClient;

        private readonly SemaphoreSlim _semaphoreSlim;


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// NotNull помечено специально, чтобы анализатор не ругался. В каждом методе должны быть провеки методом <see cref="AssertRegisterIsNull"/>
        /// </remarks>
        [NotNull] 
        private BedRegisterValues _registerValues;

        [NotNull]
        private readonly IWorkerController _workerController;

        private Worker _syncWorker;

        private BedSessionInfo _sessionInfo;

        [NotNull]
        private readonly ConcurrentQueue<Exception> _lastExceptions;

        private int _initialisingStatus;

        // ReSharper disable once NotNullMemberIsNotInitialized
        /// <inheritdoc />
        public BedUDPController([NotNull] IWorkerController workerController)
        {
            _workerController = workerController ?? throw new ArgumentNullException(nameof(workerController));
            IsConnected = false;
            _lastExceptions = new ConcurrentQueue<Exception>();
            _semaphoreSlim = new SemaphoreSlim(1, 1);
            _bedInitStepDelay = TimeSpan.FromMilliseconds(100);
            _initialisingStatus = DeviceIsNotInitialising;
        }
      
        #region Управление взаимодействием и обработка событий

        public event EventHandler OnPauseFromDeviceRequested;
        public event EventHandler OnResumeFromDeviceRequested;
        public event EventHandler OnEmeregencyStopFromDeviceRequested;
        public event EventHandler OnReverseFromDeviceRequested;

        public bool IsConnected { get; private set; }
        
        public void InitController([NotNull] IBedControllerConfig initParams)
        {
            if (initParams == null) throw new ArgumentNullException(nameof(initParams));
            var udpControllerInitParams = initParams as BedUdpControllerConfig;
            _config = udpControllerInitParams ?? throw new InvalidOperationException($"Необходимо передать объект типа {typeof(BedUdpControllerConfig)}");
            _sessionInfo = new BedSessionInfo(_config.MaxAngleX);
            _registerValues = new BedRegisterValues();
        }

        public async Task ConnectAsync()
        {
            AssertInitParams();
            if (IsConnected) throw new InvalidOperationException($"{GetType().Name} уже подключен к устройству");
            if (!IpEndPointParser.TryParse(_config.BedIpEndpoint, out _bedIpEndPoint))
            {
                throw new ArgumentException("Не верно указан адрес подключения к кровати. Требуемый формат - ip:port");
            }
            
            // немного подождем, чтобы завершилось начатое обновление данных
            var waitingTimeout = GetWaitingTimeout();
            var isFree = await _semaphoreSlim
                .WaitAsync(waitingTimeout)
                .ConfigureAwait(false);
            if (!isFree)
                throw new DeviceConnectionException(
                    $"Не удалось подключиться к инверсинному столу в течение {waitingTimeout.TotalMilliseconds} мс");

            try
            {
                // очистим перед подключением все накопленные ошибки
                while (_lastExceptions.TryDequeue(out _))
                {
                }
                
                _udpSendingClient = new UdpClient
                {
                    Client =
                    {
                        ReceiveTimeout = (int) _config.Timeout.TotalMilliseconds,
                        SendTimeout = (int) _config.Timeout.TotalMilliseconds
                    }
                }; 
                
                _udpReceivingClient = new UdpClient(_bedIpEndPoint.Port)
                {
                    Client =
                    {
                        ReceiveTimeout = (int) _config.Timeout.TotalMilliseconds,
                        SendTimeout = (int) _config.Timeout.TotalMilliseconds
                    }
                }; 
                _udpSendingClient.Connect(_bedIpEndPoint);
                _udpReceivingClient.Connect(_bedIpEndPoint);
                
                IsConnected = true;
                await UpdateRegistersValueAsync()
                    .ConfigureAwait(false);
                _syncWorker = _workerController.StartWorker(_config.UpdateDataPeriod, async () =>
                {
                    if (_initialisingStatus == DeviceIsInitialising) return;
                    
                    try
                    {
                        //todo сюды бы токен отмены
                        await UpdateRegistersValueAsync()
                            .ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        IsConnected = false;
                        _workerController.CloseWorker(_syncWorker);
                        _lastExceptions.Enqueue(e);
                    }

                });
                
            }
            catch (SocketException e)
            {
                IsConnected = false;
                throw new DeviceConnectionException("Ошибка подключения к инверсионному столу", e);
            }
            catch (ObjectDisposedException e)
            {
                IsConnected = false;
                throw new DeviceConnectionException("Ошибка подключения к инверсионному столу", e);
            }
            catch (Exception e)
            {
                IsConnected = false;
                throw new DeviceProcessingException("Ошибка в ходе обработки данных от инверсионного стола", e);
            }
        }
        
        
        private void AssertInitParams()
        {
            if (_config == null)throw new InvalidOperationException($"Контроллер не инициализирован. Необходимо сначала вызвать метод {nameof(InitController)}");
        }

        public async Task PrepareDeviceForSessionAsync()
        {
            AssertInitParams();
            AssertConnection();

            if (_initialisingStatus == DeviceIsInitialising)
                throw new InvalidOperationException("Инициализация уже выполняется");

            Interlocked.Exchange(ref _initialisingStatus, DeviceIsInitialising);
            
            // немного подождем, чтобы завершилось начатое обновление данных
            var waitingTimeout = GetWaitingTimeout();
            var isFree = await _semaphoreSlim
                .WaitAsync(waitingTimeout)
                .ConfigureAwait(false);
            if (!isFree)
                throw new DeviceConnectionException(
                    $"Не удалось подключиться к инверсинному столу в течение {waitingTimeout.TotalMilliseconds} мс");

            try
            {
                // очистим перед подключением все накопленные ошибки
                while (_lastExceptions.TryDequeue(out _))
                {
                }

                await Task.Factory.StartNew(async () =>
                    {

                        var message = new BedMessage(BedMessageEventType.Write);
                        var sendMessage = message.SetMaxAngleValueMessage(_config.MaxAngleX);
                        _udpSendingClient.Send(sendMessage, sendMessage.Length);
                        _udpReceivingClient.Receive(ref _remoteRecievedIpEndPoint);

                        await Task.Delay(_bedInitStepDelay)
                            .ConfigureAwait(false);
                        sendMessage = message.SetFreqValueMessage(_config.MovementFrequency);
                        _udpSendingClient.Send(sendMessage, sendMessage.Length);
                        _udpReceivingClient.Receive(ref _remoteRecievedIpEndPoint);

                        await Task.Delay(_bedInitStepDelay)
                            .ConfigureAwait(false);
                        sendMessage = message.SetCycleCountValueMessage((byte) _config.CyclesCount);
                        _udpSendingClient.Send(sendMessage, sendMessage.Length);
                        _udpReceivingClient.Receive(ref _remoteRecievedIpEndPoint);
                    })
                    .ConfigureAwait(false);
            }
            catch (SocketException e)
            {
                IsConnected = false;
                throw new DeviceConnectionException("Ошибка подключения к инверсионному столу", e);
            }
            catch (ObjectDisposedException e)
            {
                IsConnected = false;
                throw new DeviceConnectionException("Ошибка подключения к инверсионному столу", e);
            }
            catch (Exception e)
            {
                IsConnected = false;
                throw new DeviceProcessingException("Ошибка старта сеанса инверсионного стола", e);
            }
            finally
            {
                Interlocked.Exchange(ref _initialisingStatus, DeviceIsNotInitialising);
                _semaphoreSlim.Release();
            }
        }

        private TimeSpan GetWaitingTimeout()
        {
            return TimeSpan.FromMilliseconds(_config.UpdateDataPeriod.TotalMilliseconds * 2);
        }

        /// <summary>
        /// Установить/снять блокировку кровати (на время измерения с КМ)
        /// </summary>
        /// <param name="isBlock"></param>
        /// <returns></returns>
        /// <exception cref="DeviceConnectionException"></exception>
        //todo fix
        public async Task SetBedBlock(bool isBlock)
        {
            AssertInitParams();
            AssertConnection();
            
            // немного подождем, чтобы завершилось начатое обновление данных
            var waitingTimeout = GetWaitingTimeout();
            var isFree = await _semaphoreSlim
                .WaitAsync(waitingTimeout)
                .ConfigureAwait(false);
            if (!isFree)
                throw new DeviceConnectionException(
                    $"Не удалось подключиться к инверсинному столу в течение {waitingTimeout.TotalMilliseconds} мс");

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    var message = new BedMessage();
                    var sendMessage = message.SetBedBlockMessage(isBlock);
                    //todo а получить подтверждение
                    _udpSendingClient.Send(sendMessage, sendMessage.Length);
                }
                catch (SocketException e)
                {
                    IsConnected = false;
                    throw new DeviceConnectionException("Ошибка отключения от инверсионного стола", e);
                }
                catch (ObjectDisposedException e)
                {
                    IsConnected = false;
                    throw new DeviceConnectionException("Ошибка отключения от инверсионного стола", e);
                }
                catch (Exception e)
                {
                    IsConnected = false;
                    throw new DeviceProcessingException("Ошибка отключения от инверсионного стола", e);
                }
            }).ConfigureAwait(false);
        }
        
        /// <summary>
        /// Обновляет значения регистров с кровати
        /// </summary>
        private async Task UpdateRegistersValueAsync()
        {
            AssertConnection();
            
            
            // немного подождем, чтобы завершилось начатое обновление данных
            var waitingTimeout = GetWaitingTimeout();
            var isFree = await _semaphoreSlim
                .WaitAsync(waitingTimeout)
                .ConfigureAwait(false);
            if (!isFree)
                throw new DeviceConnectionException(
                    $"Не удалось подключиться к инверсинному столу в течение {waitingTimeout.TotalMilliseconds} мс");
            
            await Task.Factory.StartNew(() => {
                try
                { 
                    //здесь запрос данных и их парсинг 
                    var message = new BedMessage(BedMessageEventType.ReadAll);
                    var getAllRegister = message.GetAllRegisterMessage();
                    _udpSendingClient.Send(getAllRegister, getAllRegister.Length);
                    var receiveMessage = _udpReceivingClient.Receive(ref _remoteRecievedIpEndPoint);
                    var previouslRegisterValues = _registerValues;
                    _registerValues = message.GetAllRegisterValues(receiveMessage);
                    
                    if ((previouslRegisterValues.BedStatus == BedStatus.SessionStarted 
                         || previouslRegisterValues.BedStatus == BedStatus.Pause)
                        && _registerValues.BedStatus == BedStatus.Reverse)
                    {
                        OnReverseFromDeviceRequested?.Invoke(this, EventArgs.Empty);
                    }
                    if (previouslRegisterValues.BedStatus == BedStatus.SessionStarted &&
                        _registerValues.BedStatus == BedStatus.Pause)
                    {
                        OnPauseFromDeviceRequested?.Invoke(this,EventArgs.Empty);
                    }

                    if (previouslRegisterValues.BedStatus == BedStatus.Pause &&
                        _registerValues.BedStatus == BedStatus.SessionStarted)
                    {
                        OnResumeFromDeviceRequested?.Invoke(this, EventArgs.Empty);
                    }
                    if (previouslRegisterValues.BedStatus != BedStatus.EmergencyStop &&
                        _registerValues.BedStatus == BedStatus.EmergencyStop)
                    {
                        OnEmeregencyStopFromDeviceRequested?.Invoke(this,EventArgs.Empty);
                    }
                }
                catch (SocketException e)
                {
                    IsConnected = false;
                    throw new DeviceConnectionException("Ошибка соедининия с инверсионным столом", e);
                }
                catch (ObjectDisposedException e)
                {
                    IsConnected = false;
                    throw new DeviceConnectionException("Ошибка соедининия с инверсионным столом", e);
                }
                catch (Exception e)
                {
                    IsConnected = false;
                    throw new DeviceProcessingException("Ошибка получения данных от инверсионного стола", e);
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }).ConfigureAwait(false);
        }
        
        public Task DisconnectAsync()
        {
            AssertConnection();
            try
            {
                _workerController.CloseWorker(_syncWorker);
                _udpSendingClient?.Close();
                _udpSendingClient?.Dispose();
                _udpReceivingClient?.Close();
                _udpReceivingClient?.Dispose();

                return Task.CompletedTask;
            }
            catch (SocketException e)
            {
                IsConnected = false;
                throw new DeviceConnectionException("Ошибка отключения от инверсионного стола", e);
            }
            catch (ObjectDisposedException e)
            {
                IsConnected = false;
                throw new DeviceConnectionException("Ошибка отключения от инверсионного стола", e);
            }
            catch (Exception e)
            {
                IsConnected = false;
                throw new DeviceProcessingException("Ошибка отключения от инверсионного стола", e);
            }
        }

        private void AssertConnection()
        {
            if (!IsConnected || _udpSendingClient == null || _udpReceivingClient == null) throw new DeviceConnectionException(
                $"Соединение с инверсионным столом не установлено. Установите соединение с помощью метода {nameof(ConnectAsync)}");
        }
        
        //todo fix sending
        public async Task ExecuteCommandAsync(BedControlCommand command)
        {
            AssertInitParams();
            AssertConnection();
            
            var isFree = await _semaphoreSlim
                .WaitAsync(_config.Timeout)
                .ConfigureAwait(false);
            if (!isFree) throw new DeviceConnectionException(
                $"Не удалось подключиться к инверсионному столу в течение {_config.Timeout.TotalMilliseconds}");

            await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var message = new BedMessage(BedMessageEventType.Write);
                        var sendMessage = message.GetBedCommandMessage(command);
                        _udpSendingClient.Send(sendMessage, sendMessage.Length);
                        //todo зачем после каждой отправки мы ожидаем ответа? Мы делаем из UDP свой TCP
                        _udpReceivingClient.Receive(ref _remoteRecievedIpEndPoint);
                    }
                    catch (SocketException e)
                    {
                        IsConnected = false;
                        throw new DeviceConnectionException("Ошибка соединения с инверсионным столом", e);
                    }
                    catch (ObjectDisposedException e)
                    {
                        IsConnected = false;
                        throw new DeviceConnectionException("Ошибка соединения с инверсионным столом", e);
                    }
                    catch (Exception e)
                    {
                        IsConnected = false;
                        throw new DeviceProcessingException("Ошибка отправки команды инверсионному столу", e);
                    }
                    finally
                    {
                        _semaphoreSlim.Release();
                    }
                })
                .ConfigureAwait(false);
            
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
                throw new DeviceConnectionException("Ошибка во взаимодействии с инверсионным столом", agregatedException);
            }
            throw new DeviceProcessingException("Ошибка обработки данных от инверсионного стола", agregatedException);
        }
        
        #endregion

        #region Получение данных

        public Task<BedMovingStatus> GetBedMovingStatusAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            throw new NotImplementedException();
        }

        private void AssertRegisterIsNull()
        {
            if (_registerValues == null) 
                throw new InvalidOperationException(
                    $"Не было получено данных от кровати. Необходмо выполнить подключение методом {nameof(ConnectAsync)} или дождаться получения данных");
        }

        public async Task<TimeSpan> GetCycleDurationAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            var iterationsCount = await GetIterationsCountAsync()
                .ConfigureAwait(false);
            return  _sessionInfo.GetCycleDuration(iterationsCount);
        }

        public Task<short> GetCyclesCountAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            return Task.FromResult(_config.CyclesCount);
        }

        public Task<short> GetCurrentCycleNumberAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            return Task.FromResult(_registerValues.CurrentCycle);
        }

        public Task<short> GetIterationsCountAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            return Task.FromResult(_sessionInfo.GetIterationsCount(_config.MaxAngleX));
        }

        public Task<short> GetCurrentIterationAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            return Task.FromResult(_registerValues.CurrentIteration);
        }

        public async Task<short> GetNextIterationNumberForPressureMeasuringAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            var currentIteation = await GetCurrentIterationAsync()
                .ConfigureAwait(false);
            return  _sessionInfo.GetNextIterationNumberForPressureMeasuring(currentIteation);
        }

        public async Task<short> GetNextIterationNumberForCommonParamsMeasuringAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            var currentIteation = await GetCurrentIterationAsync()
                .ConfigureAwait(false);
            return _sessionInfo.GetNextIterationNumberForCommonParamsMeasuring(currentIteation);
        }

        public async Task<short> GetNextIterationNumberForEcgMeasuringAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            var currentIteation = await GetCurrentIterationAsync()
                .ConfigureAwait(false);
            return  _sessionInfo.GetNextIterationNumberForEcgMeasuring(currentIteation);
        }

        public Task<BedStatus> GetBedStatusAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            return Task.FromResult(_registerValues.BedStatus);
        }

        public Task<TimeSpan> GetRemainingTimeAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            return Task.FromResult(_registerValues.RemainingTime);
        }

        public Task<TimeSpan> GetElapsedTimeAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            return Task.FromResult(_registerValues.ElapsedTime);
        }

        public Task<StartFlag> GetStartFlagAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            throw new NotImplementedException();
        }

        public Task<ReverseFlag> GetReverseFlagAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            throw new NotImplementedException(); //todo не помню где лежит
        }

        public Task<float> GetAngleXAsync() 
        {
            
            RiseExceptions();
            AssertRegisterIsNull();
            return Task.FromResult(_registerValues.BedTargetAngleX);

        }

        #endregion

        public void Dispose()
        {
            _workerController.CloseWorker(_syncWorker);
            try
            {
                _udpSendingClient?.Close();
                _udpReceivingClient?.Close();
            }
            finally 
            {
                _udpSendingClient?.Dispose();
                _udpReceivingClient?.Dispose();
            }
        }

        public Guid DeviceId => InversionTableV2UdpDeviceId.DeviceId;
        public Guid DeviceTypeId => InversionTableDeviceTypeId.DeviceTypeId;
    }
}
