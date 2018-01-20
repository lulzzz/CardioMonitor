using System;
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

        public BedUDPController([NotNull] IWorkerController workerController)
        {
            _workerController = workerController ?? throw new ArgumentNullException(nameof(workerController));
            IsConnected = false;
        }
       


        #endregion

        #region Управление взаимодействием и обработка событий

        public event EventHandler OnStartFromDeviceRequested;
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
                        //todo сюды бы токен синхронизации
                        await UpdateRegistersValueAsync()
                            .ConfigureAwait(false);
                        await RiseEventOnCommandFromDeviceAsync()
                            .ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        IsConnected = false;
                        //todo logging
                    }
                    
                });
                IsConnected = true;
            }
            catch (Exception)
            {
                IsConnected = false;
                throw;
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
            //todo здесь получение массива с регистрами и обновление результатов в _registerList
            await Task.Yield();
            
            
            _registerValues = new BedRegisterValues();
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
            await Task.Yield();
            _workerController.CloseWorker(_syncWorker);
            _udpClient?.Dispose();
        }

        private void AssertConnection()
        {
            if (!IsConnected) throw new InvalidOperationException($"Соединение с кроватью не уставнолено. Установите соединение с помощью метода {nameof(ConnectAsync)}");
        }
        
        public async Task ExecuteCommandAsync(BedControlCommand command)
        {
            AssertConnection();

            await Task.Yield();
        }

        #endregion

        #region Получение данных

        public Task<BedMovingStatus> GetBedMovingStatusAsync()
        {
            AssertRegisterIsNull();
            throw new NotImplementedException();
        }

        private void AssertRegisterIsNull()
        {
            if (_registerValues == null) 
                throw new InvalidOperationException($"Не было получено данных от кровати. Необходмо выполнить подключение методом {nameof(ConnectAsync)} или дождаться получения данных");
        }

        public Task<TimeSpan> GetCycleDurationAsync()
        {
            AssertRegisterIsNull();
            throw new NotImplementedException();
        }

        public Task<short> GetCyclesCountAsync()
        {
            AssertRegisterIsNull();
            throw new NotImplementedException();
        }

        public Task<short> GetCurrentCycleNumberAsync()
        {
            AssertRegisterIsNull();
            throw new NotImplementedException();
        }

        public Task<short> GetIterationsCountAsync()
        {
            AssertRegisterIsNull();
            throw new NotImplementedException();
        }

        public Task<short> GetCurrentIterationAsync()
        {
            AssertRegisterIsNull();
            throw new NotImplementedException();
        }

        public Task<short> GetNextIterationNumberForPressureMeasuringAsync()
        {
            AssertRegisterIsNull();
            throw new NotImplementedException();
        }

        public Task<short> GetNextIterationNumberForCommonParamsMeasuringAsync()
        {
            AssertRegisterIsNull();
            throw new NotImplementedException();
        }

        public Task<short> GetNextIterationNumberForEcgMeasuringAsync()
        {
            AssertRegisterIsNull();
            throw new NotImplementedException();
        }

        public Task<TimeSpan> GetRemainingTimeAsync()
        {
            AssertRegisterIsNull();
            throw new NotImplementedException();
        }

        public Task<TimeSpan> GetElapsedTimeAsync()
        {
            AssertRegisterIsNull();
            throw new NotImplementedException();
        }

        public Task<StartFlag> GetStartFlagAsync()
        {
            AssertRegisterIsNull();
            throw new NotImplementedException();
        }

        public Task<ReverseFlag> GetReverseFlagAsync()
        {
            AssertRegisterIsNull();
            throw new NotImplementedException();
        }

        public async Task<double> GetAngleXAsync() //так как запрашивается просто из регистра - думаю таска тут не нужна
        {
            
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
    }
}
