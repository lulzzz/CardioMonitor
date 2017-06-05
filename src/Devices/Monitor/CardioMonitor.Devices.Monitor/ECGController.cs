using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Devices.Monitor
{
    /// <summary>
    /// Класс для получения ЭКГ с кардиомонитора
    /// </summary>
    public class ECG
    {
        /// <summary>
        /// размер входного пакета от кардиомонитора в байтах
        /// </summary>
        private int _inputPacketSize = 64;
        public void ECGReceive(NetworkStream ns)
        {
           byte[] inputPacket = new byte[_inputPacketSize];
            ns.Read(inputPacket, 0, _inputPacketSize);

        }

    }
}
