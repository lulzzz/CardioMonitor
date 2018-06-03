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
        //todo оставил, чтобы пока помнить адресс
        //private IPEndPoint _bedIpEndPoint = new IPEndPoint(IPAddress.Parse("192.168.56.3"), 7777);

        /// <remarks>
        /// NotNull помечено специально, чтобы анализатор не ругался. В каждом методе должны быть провеки методом <see cref="AssertInitParams"/>
        /// </remarks>
        [NotNull] 
        private BedUdpControllerConfig _config;

        [CanBeNull]
        private UdpClient _udpClient;

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);


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

        public BedUDPController([NotNull] IWorkerController workerController)
        {
            _workerController = workerController ?? throw new ArgumentNullException(nameof(workerController));
            IsConnected = false;
            _lastExceptions = new ConcurrentQueue<Exception>();
        }
      
       

        #region Управление взаимодействием и обработка событий

        public event EventHandler OnPauseFromDeviceRequested;
        public event EventHandler OnResumeFromDeviceRequested;
        public event EventHandler OnEmeregencyStopFromDeviceRequested;
        public event EventHandler OnReverseFromDeviceRequested;

        public bool IsConnected { get; private set; }
        
        public void Init([NotNull] IBedControllerConfig initParams)
        {
            if (initParams == null) throw new ArgumentNullException(nameof(initParams));
            var udpControllerInitParams = initParams as BedUdpControllerConfig;
            _config = udpControllerInitParams ?? throw new InvalidOperationException($"Необходимо передать объект типа {typeof(BedUdpControllerConfig)}");
            _sessionInfo = new BedSessionInfo(_config.MaxAngleX);
        }

        public async Task ConnectAsync()
        {
            AssertInitParams();
            if (IsConnected) throw new InvalidOperationException($"{GetType().Name} уже подключен к устройству");

            try
            {
                await Task.Yield();
                // очистим перед подключением все накопленные ошибки
                while (_lastExceptions.TryDequeue(out var _))
                {
                }
                _udpClient = new UdpClient(7777); //todo в конфиги


                var endPoint = IpEndPointParser.Parse(_config.BedIpEndpoint);
                _udpClient.Connect(endPoint);
                IsConnected = true;
                await UpdateRegistersValueAsync()
                    .ConfigureAwait(false);
                await SetInitParamsAsync().ConfigureAwait(false);
                await RiseEventOnCommandFromDeviceAsync()
                    .ConfigureAwait(false);
                _syncWorker = _workerController.StartWorker(_config.UpdateDataPeriod, async () =>
                {
                    try
                    {
                        //todo сюды бы токен отмены
                        await UpdateRegistersValueAsync()
                            .ConfigureAwait(false);
                        await RiseEventOnCommandFromDeviceAsync()
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
            if (_config == null)throw new InvalidOperationException($"Контроллер не инициализирован. Необходимо сначала вызвать метод {nameof(Init)}");
        }
        
        
        /// <summary>
        /// Отправить стартовые параметры на кровать перед запуском
        /// </summary>
        private async Task SetInitParamsAsync()
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                AssertConnection();
                if (_udpClient == null) throw new DeviceConnectionException("Ошибка подключения к инверсионному столу");
                await Task.Yield();
                BedMessage message = new BedMessage(BedMessageEventType.Write);
                var sendMessage = message.SetMaxAngleValueMessage(_config.MaxAngleX);
                await _udpClient.SendAsync(sendMessage, sendMessage.Length);
                var receiveMessage = await _udpClient.ReceiveAsync();
                Thread.Sleep(100);
                //todo здесь лучше сделать паузу ~100mc 
                sendMessage = message.SetFreqValueMessage(_config.MovementFrequency);
                await _udpClient.SendAsync(sendMessage, sendMessage.Length);
                receiveMessage = await _udpClient.ReceiveAsync();
                Thread.Sleep(100);
                //todo здесь лучше сделать паузу ~100mc 
                //sendMessage = message.SetCycleCountValueMessage((byte) _config.CyclesCount);
                //await _udpClient.SendAsync(sendMessage, sendMessage.Length);
                //receiveMessage = await _udpClient.ReceiveAsync();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        /// <summary>
        /// Установить/снять блокировку кровати (на время измерения с КМ)
        /// </summary>
        /// <param name="isBlock"></param>
        /// <returns></returns>
        /// <exception cref="DeviceConnectionException"></exception>
        public async Task SetBedBlock(bool isBlock)
        {
            AssertConnection();
            if (_udpClient == null) throw new DeviceConnectionException("Ошибка подключения к инверсионному столу");
            await Task.Yield();
            BedMessage message = new BedMessage();
            var sendMessage = message.SetBedBlockMessage(isBlock);
            await _udpClient.SendAsync(sendMessage, sendMessage.Length);
        }
        
        /// <summary>
        /// Обновляет значения регистров с кровати
        /// </summary>
        private async Task UpdateRegistersValueAsync()
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                AssertConnection();
                if (_udpClient == null)
                    throw new DeviceConnectionException("Ошибка подключения к инверсионному столу"); //todo 
                //todo здесь получение массива с регистрами и обновление результатов в _registerList
                //todo никакой обработки ошибок делать не надо
                await Task.Yield();
                //здесь короче запрос данных и их парсинг 
                var message = new BedMessage(BedMessageEventType.ReadAll);
                var getAllRegister = message.GetAllRegisterMessage();
                await _udpClient.SendAsync(getAllRegister, getAllRegister.Length);
                var receiveMessage = await _udpClient.ReceiveAsync();
                _registerValues = message.GetAllRegisterValues(receiveMessage.Buffer);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
        
        /// <summary>
        /// Вызывает подписчиков событий на команды от кровати
        /// </summary>
        private async Task RiseEventOnCommandFromDeviceAsync()
        {
            await Task.Yield();
        }

        public async Task DisconnectAsync()
        {
            AssertConnection();
            try
            {
                await Task.Yield();
                _workerController.CloseWorker(_syncWorker);
                _udpClient?.Dispose();

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
            if (!IsConnected) throw new InvalidOperationException($"Соединение с кроватью не установлено. Установите соединение с помощью метода {nameof(ConnectAsync)}");
        }
        
        public async Task ExecuteCommandAsync(BedControlCommand command)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                RiseExceptions();
                AssertConnection();
                if (_udpClient == null) throw new DeviceConnectionException("Ошибка подключения к инверсионному столу");
                await Task.Yield();
                BedMessage message = new BedMessage(BedMessageEventType.Write);
                var sendMessage = message.GetBedCommandMessage(command);
                await _udpClient.SendAsync(sendMessage, sendMessage.Length);
                var receiveMessage = await _udpClient.ReceiveAsync();
            }
            finally
            {
                semaphoreSlim.Release();
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
                throw new InvalidOperationException($"Не было получено данных от кровати. Необходмо выполнить подключение методом {nameof(ConnectAsync)} или дождаться получения данных");
        }

        public async Task<TimeSpan> GetCycleDurationAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            await Task.Yield();
            return   _sessionInfo.GetCycleDuration(await GetIterationsCountAsync());
        }

        public async Task<short> GetCyclesCountAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            await Task.Yield();
            return  _config.CyclesCount;
        }

        public async Task<short> GetCurrentCycleNumberAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            await Task.Yield();
            return  _registerValues.CurrentCycle;
        }

        public async Task<short> GetIterationsCountAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            await Task.Yield();
            return _sessionInfo.GetIterationsCount(_config.MaxAngleX);
        }

        public async Task<short> GetCurrentIterationAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            await Task.Yield();
            return  _registerValues.CurrentIteration;
        }

        public async Task<short> GetNextIterationNumberForPressureMeasuringAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            await Task.Yield();
            return  _sessionInfo.GetNextIterationNumberForPressureMeasuring(await GetCurrentIterationAsync());
        }

        public async Task<short> GetNextIterationNumberForCommonParamsMeasuringAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            await Task.Yield();
            return _sessionInfo.GetNextIterationNumberForCommonParamsMeasuring(await GetCurrentIterationAsync());
        }

        public async Task<short> GetNextIterationNumberForEcgMeasuringAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            await Task.Yield();
            return  _sessionInfo.GetNextIterationNumberForEcgMeasuring(await GetCurrentIterationAsync());
        }

        public async Task<TimeSpan> GetRemainingTimeAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            await Task.Yield();
            return  _registerValues.RemainingTime;
        }

        public async Task<TimeSpan> GetElapsedTimeAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            await Task.Yield();
            return  _registerValues.ElapsedTime;
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

        public async Task<float> GetAngleXAsync() 
        {
            
            RiseExceptions();
            AssertRegisterIsNull();
            await Task.Yield();
            return _registerValues.BedTargetAngleX;

        }

        #endregion

        public void Dispose()
        {
            _workerController.CloseWorker(_syncWorker);
            _udpClient?.Dispose();
        }

        public Guid DeviceId => InversionTableV2UdpDeviceId.DeviceId;
        public Guid DeviceTypeId => InversionTableDeviceTypeId.DeviceTypeId;
    }
}
