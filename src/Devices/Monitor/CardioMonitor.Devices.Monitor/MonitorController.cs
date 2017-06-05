using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Devices.Monitor.Infrastructure;

namespace CardioMonitor.Devices.Monitor
{
    /// <summary>
    /// класс для работы с кардиомонитором МИТАР - новая версия
    /// </summary>
    public class MonitorController : IMonitorController
    {
        private UdpClient _udpClient;
        private readonly int localUdpPort = 30304;
        private IPAddress remoteMonitorIpAddress;
        private readonly int remoteMonitorTcpPort = 9761;

        private TcpClient _tcpClient;
        public Task<bool> PumpCuffAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PatientParams> GetPatientParamsAsync()
        {
            throw new NotImplementedException();
        }
        
        //черновой вариант - отрефачить после тестов с новым монитором
        public void Listner()
        {
            try
            {
                IPEndPoint localUdpIp = new IPEndPoint(IPAddress.Any, localUdpPort);
                _udpClient = new UdpClient(localUdpIp);
                while (true)
                {
                    IPEndPoint remoteUdpIp = null;
                    byte[] message = _udpClient.Receive(ref remoteUdpIp);
                    remoteMonitorIpAddress = remoteUdpIp.Address;
                    ConnectToMonitor(new IPEndPoint(remoteMonitorIpAddress,remoteMonitorTcpPort));
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
                NetworkStream ns = _tcpClient.GetStream();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}
