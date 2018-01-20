using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CardioMonitor.Devices.Monitor.Infrastructure;

namespace CardioMonitor.Devices.Monitor
{
    /// <summary>
    /// класс для работы с кардиомонитором МИТАР - новая версия
    /// </summary>
    public class MitarMonitorController : IMonitorController
    {
        
        private UdpClient _udpClient;
        private readonly int localUdpPort = 30304;
        private IPAddress remoteMonitorIpAddress;
        private readonly int remoteMonitorTcpPort = 9761;

        private NetworkStream _stream;
        private TcpClient _tcpClient;

        #region Управление контроллером
        
        /// <summary>
        /// наличие подключения к КардиоМонитору
        /// </summary>
        public bool IsConnected { get; private set; }

        public void Init(IMonitorControllerInitParams initParams)
        {
            throw new NotImplementedException();
        }

        public Task ConnectAsync()
        {
            throw new NotImplementedException();
        }

        public Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }
        
        public void Dispose()
        {
            _udpClient?.Dispose();
            _stream?.Dispose();
            _tcpClient?.Dispose();
        }

        #endregion


        #region Получение данных

        public Task PumpCuffAsync()
        {
            throw new NotImplementedException();
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
        
      

    
        //черновой вариант - отрефачить после тестов с новым монитором
        public void Listner()
        {
            try
            {
                if (IsMonitorConnected) throw new Exception("Монитор уже подключен");
                IPEndPoint localUdpIp = new IPEndPoint(IPAddress.Any, localUdpPort);
                _udpClient = new UdpClient(localUdpIp);
                while (true)
                {
                    IPEndPoint remoteUdpIp = null;
                    byte[] message = _udpClient.Receive(ref remoteUdpIp);
                    remoteMonitorIpAddress = remoteUdpIp.Address;
                    ConnectToMonitor(new IPEndPoint(remoteMonitorIpAddress, remoteMonitorTcpPort));
                }

            }
            
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// установка соединения с монитором.
        /// К этому моменту мы установили его IP
        /// </summary>
        /// <param name="remoteMonitorEndPoint"> IP / порт монитора </param>
        public void ConnectToMonitor(IPEndPoint remoteMonitorEndPoint)
        {
            try
            {
                _tcpClient = new TcpClient();
                _tcpClient.Connect(remoteMonitorEndPoint);
                _stream = _tcpClient.GetStream();
                IsMonitorConnected = true;
            }
            catch (SocketException)
            {
                //todo throw Exception - ошибка подключения
                IsMonitorConnected = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }


       
    }
}
