using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using CardioMonitor.Devices.Bed.Infrastructure;
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
        private BedUdpControllerInitParams _initParams;

        [CanBeNull]
        private UdpClient _udpClient;

        #region Old

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
       


        #endregion

        #region Управление взаимодействием и обработка событий

        public event EventHandler OnPauseFromDeviceRequested;
        public event EventHandler OnResumeFromDeviceRequested;
        public event EventHandler OnEmeregencyStopFromDeviceRequested;
        public event EventHandler OnReverseFromDeviceRequested;

        public bool IsConnected { get; private set; }
        
        public void Init([NotNull] IBedControllerInitParams initParams)
        {
            if (initParams == null) throw new ArgumentNullException(nameof(initParams));
            var udpControllerInitParams = initParams as BedUdpControllerInitParams;
            _initParams = udpControllerInitParams ?? throw new InvalidOperationException($"Необходимо передать объект типа {typeof(BedUdpControllerInitParams)}");
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
                _udpClient = new UdpClient();
                _udpClient.Connect(_initParams.BedIPEndpoint);
                await UpdateRegistersValueAsync()
                    .ConfigureAwait(false);
                await RiseEventOnCommandFromDeviceAsync()
                    .ConfigureAwait(false);
                _syncWorker = _workerController.StartWorker(_initParams.UpdateDataPeriod, async () =>
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
                IsConnected = true;
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
            if (_initParams == null)throw new InvalidOperationException($"Контроллер не инициализирован. Необходимо сначала вызвать метод {nameof(Init)}");
        }
        
        /// <summary>
        /// Обновляет значения регистров с кровати
        /// </summary>
        private async Task UpdateRegistersValueAsync()
        {
            AssertConnection();
            if (_udpClient == null) throw new DeviceConnectionException("Ошибка подключения к инверсионному столу"); //todo 
            //todo здесь получение массива с регистрами и обновление результатов в _registerList
            //todo никакой обработки ошибок делать не надо
            await Task.Yield();
            //здесь короче запрос данных и их парсинг 
            var message = new BedMessage(BedMessageEventType.ReadAll, 0 ); 
            await _udpClient.SendAsync(message.GetAllRegisterMessage(), message.GetAllRegisterMessage().Length);
            var receiveMessage = await _udpClient.ReceiveAsync();
            _registerValues = message.SetAllRegisterValues(receiveMessage.Buffer);
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
            RiseExceptions();
            AssertConnection();

            await Task.Yield();
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
            return   _sessionInfo.GetCycleDuration();
        }

        public async Task<short> GetCyclesCountAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            await Task.Yield();
            return  (short)_initParams.CyclesCount;
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
            return _sessionInfo.GetIterationsCount();
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
            return  _sessionInfo.GetNextIterationNumberForPressureMeasuring();
        }

        public async Task<short> GetNextIterationNumberForCommonParamsMeasuringAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            await Task.Yield();
            return _sessionInfo.GetNextIterationNumberForCommonParamsMeasuring();
        }

        public async Task<short> GetNextIterationNumberForEcgMeasuringAsync()
        {
            RiseExceptions();
            AssertRegisterIsNull();
            await Task.Yield();
            return  _sessionInfo.GetNextIterationNumberForEcgMeasuring();
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

        public Guid DeviceId => InversionTableUpdDeviceId.DeviceId;
        public Guid DeviceTypeId => InversionTableDeviceTypeId.DeviceTypeId;
    }
}
