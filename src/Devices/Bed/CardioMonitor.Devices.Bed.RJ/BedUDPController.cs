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
        private BedRegisterList _registerList;
        /// <summary>
        /// настройки подключения к кровати
        /// </summary>
        private IPEndPoint _bedIpEndPoint = new IPEndPoint(IPAddress.Parse("192.168.56.3"), 7777);
        private UdpClient _udpClient;

        /// <summary>
        /// Подключена ли кровать
        /// </summary>
        public bool IsBedConnect { get; set; } = false;
        
        /// <summary>
        /// Метод для подключения к кровати
        /// </summary>
        public void BedConnection()
        {
            try
            {
                _udpClient.Connect(_bedIpEndPoint);
                IsBedConnect = true;
            }
            catch (SocketException e)
            {
                IsBedConnect = false;
               throw new SocketException();
            }
            
        }

        /// <summary>
        /// обновить значения регистров с кровати
        /// </summary>
        public void UpdateRegistersValue()
        {
            //todo здесь получение массива с регистрами и обновление результатов в _registerList
        }


        private byte[] MessageConstructor(/*todo registerType*/)
        {
            byte[] outputMessage = new byte[3];

            return outputMessage;
        }

        private void SendMessage(byte[] message)
        {
            _udpClient.Send(message, message.Length); //todo синхронная передача в блокирующем режиме - лучше через таску с таймаутом SendAsync
        }

        public BedStatus GetBedStatus()
        {
           return (BedStatus)_registerList.BedStatus;
        }

        public BedMovingStatus GetBedMovingStatus()
        {
            throw new NotImplementedException();
        }

        public Task<TimeSpan> GetCycleDurationAsync() //здесь по идее тоже, ибо операция расчета выполняется при старте один раз
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

        public Task<double> GetAngleXAsync() //так как запрашивается просто из регистра - думаю таска тут не нужна
        {
            return _registerList.BedTargetAngleX;
           
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
