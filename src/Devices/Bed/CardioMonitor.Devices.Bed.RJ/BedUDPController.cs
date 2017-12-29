using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CardioMonitor.Devices.Bed.Infrastructure;

namespace CardioMonitor.Devices.Bed.UDP
{
    /// <summary>
    /// Работа с кроватью по сети
    /// протокол передачи посылок - UDP
    /// </summary>
    public class BedUDPController : IBedController 
    {
        /// <summary>
        /// настройки подключения к кровати
        /// </summary>
        private IPEndPoint _bedIpEndPoint = new IPEndPoint(IPAddress.Parse("192.168.56.3"), 7);
        private UdpClient _udpClient;

        /// <summary>
        /// Метод для подключения к кровати
        /// </summary>
        /// <param name="remoteIpEndPoint"> Ip и порт кровати </param>
        public void BedConnection(IPEndPoint remoteIpEndPoint)
        {
            _udpClient.Connect(remoteIpEndPoint);
        }

        private byte[] MessageConstructor()
        {
            byte[] outputMessage = new byte[3];

            return outputMessage;
        }

        private void SendMessage(byte[] message)
        {
            
        }

        public BedStatus GetBedStatus()
        {
            return BedStatus.Ready;
        }

        public BedMovingStatus GetBedMovingStatus()
        {
            throw new NotImplementedException();
        }

        public Task<TimeSpan> GetCycleDurationAsync()
        {
            throw new NotImplementedException();
        }

        public void UpdateFlags()
        {
            throw new NotImplementedException();
        }

        public StartFlag GetStartFlag()
        {
            throw new NotImplementedException();
        }

        public ReverseFlag GetReverseFlag()
        {
            throw new NotImplementedException();
        }

        public Task<double> GetAngleXAsync()
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public void ExecuteCommand(BedControlCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
