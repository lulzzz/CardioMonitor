using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Devices.Monitor
{
    public class MonitorTCPClient
    {
        public delegate void ReceiveDataCallback();

        public delegate void DisconnectedCallback();

        public delegate void ErrorCallback(); //todo обдумать необходимость

        /// <summary>
        /// 
        /// </summary>
        public ReceiveDataCallback OnReceiveData { get; set; }
        
        public DisconnectedCallback OnDisconnected { get; set; }

        public Queue<byte[]> InputMessageQueue { get; set; }

        private TcpClient _tcpClient;

        public void Connect(IPEndPoint ipAdress)
        {
            try
            {
                _tcpClient.Connect(ipAdress);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void DisConnect()
        {
            try
            {
                _tcpClient?.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void WaitForData()
        {
            try
            {
                var ns = _tcpClient.GetStream();
                byte[] inputMessage = new byte[];
                ns.BeginRead()
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


    }
}
