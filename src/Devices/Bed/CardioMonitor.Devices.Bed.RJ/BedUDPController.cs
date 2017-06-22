using System.Net;
using System.Net.Sockets;
using CardioMonitor.Devices.Bed.Infrastructure;

namespace CardioMonitor.Devices.Bed.UDP
{
    /// <summary>
    /// Работа с кроватью по сети
    /// протокол передачи посылок - UDP
    /// </summary>
    public class BedUDPController : IBedController 
    {
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
            
        }


    }
}
