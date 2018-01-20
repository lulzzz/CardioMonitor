using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CardioMonitor.Devices.Monitor.Infrastructure;
using JetBrains.Annotations;

namespace CardioMonitor.Devices.Monitor
{
    /// <summary>
    /// Параметры подкючения к монитору МИТАР
    /// </summary>
    public class MitarMonitorControlerInitParams : IMonitorControllerInitParams
    {
        /// <summary>
        /// Порт, по которому будут приходить broadcast сообщения от монитора
        /// </summary>
        public int MonitorBroadcastUdpPort { get; }
        
        /// <summary>
        /// Порт для TCP подключения к монитору
        /// </summary>
        /// <remarks>
        /// IP можно узнать в результе broadcast'a от устройства, который надо слушать по порту <see cref="MonitorBroadcastUdpPort"/>
        /// </remarks>
        public int MonitorTcpPort { get; }
    }
    
    /// <summary>
    /// Контроллер взаимодействия с кардиомонитором МИТАР
    /// </summary>
    public class MitarMonitorController : IMonitorController
    {
        //todo оставил, чтобы пока помнить адресс
//        private readonly int localUdpPort = 30304;
//        private IPAddress remoteMonitorIpAddress;
//        private readonly int remoteMonitorTcpPort = 9761;

        private NetworkStream _stream;
        private TcpClient _tcpClient;
        
        /// <remarks>
        /// NotNull помечено специально, чтобы анализатор не ругался. В каждом методе должны быть провеки методом <see cref="AssertInitParams"/>
        /// </remarks>
        [NotNull]
        private MitarMonitorControlerInitParams _initParams;

        public MitarMonitorController()
        {
            IsConnected = false;
        }
        
        #region Управление контроллером
        
        /// <summary>
        /// наличие подключения к КардиоМонитору
        /// </summary>
        public bool IsConnected { get; private set; }

        
        public void Init(IMonitorControllerInitParams initParams)
        {
            if (initParams == null) throw new ArgumentNullException(nameof(initParams));

            var mitarInitParams = initParams as MitarMonitorControlerInitParams;
            _initParams = mitarInitParams ?? throw new ArgumentException($"{nameof(initParams)} должен быть типа {typeof(MitarMonitorControlerInitParams)}");
        }

        public async Task ConnectAsync()
        {
            AssertInitParams();
            
            if (IsConnected) throw new InvalidOperationException($"{GetType().Name} уже подключен к устройству");

            try
            {
                var monitorIpAddress = await DiscoverMonitorIpAddressAsync()
                    .ConfigureAwait(false);
                
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(monitorIpAddress, _initParams.MonitorTcpPort)
                    .ConfigureAwait(false);
                _stream = _tcpClient.GetStream();
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

        public async Task DisconnectAsync()
        {
            await Task.Yield();
            AssertConnection();
            _stream.Dispose();
            _stream = null;
            _tcpClient.Dispose();
            _tcpClient = null;
        }
        
        
        public void Dispose()
        {
            _stream?.Dispose();
            _tcpClient?.Dispose();
        }

        #endregion

        #region Получение данных

        public Task PumpCuffAsync()
        {
            throw new NotImplementedException();
        }
        
        private void AssertConnection()
        {
            if (!IsConnected) throw new InvalidOperationException($"Соединение с монитором не уставнолено. Установите соединение с помощью метода {nameof(ConnectAsync)}");
        }

        public Task<PatientCommonParams> GetPatientCommonParamsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PatientPressureParams> GetPatientPressureParamsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PatientEcgParams> GetPatientEcgParamsAsync(TimeSpan duration)
        {
            return Task.Factory.StartNew(() =>
            {
                var  patientECG = new PatientEcgParams();
                return patientECG;
            });
        }

        #endregion
    }
}
