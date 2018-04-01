using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardioMonitor.Devices.Bed.Infrastructure;
using static CardioMonitor.Devices.Bed.UDP.BedRegisterPosition;

namespace CardioMonitor.Devices.Bed.UDP
{
    public class BedMessage
    {
        /// <summary>
        /// Маркер - идентификатор начала пакета
        /// </summary>
        private  char _startMessageMarker = '$';

        /// <summary>
        /// ID устройства - на текущий момент всегда == 1
        /// </summary>
        private byte _idDevice = 1;  

        /// <summary>
        /// тип события: информация читается или записывается
        /// </summary>
        private BedMessageEventType _eventType;

        /// <summary>
        /// размер пакета данных - в словах (байты * 2)
        /// </summary>
        private byte messageLength;

        /// <summary>
        /// Адрес регистра, из которого/ с которого/ в который производится чтение/запись
        /// </summary>
        private int _registerAddress;

        /// <summary>
        /// Данные из сообщения
        /// </summary>
        private byte[] _messageData;

        public BedMessage(){}
        
        public BedMessage(BedMessageEventType eventType,  byte messageLength = 2) //при запросе всех регистров - Length всегда 2 (адрес и дата по 1 слову)
        {
            _eventType = eventType;
            this.messageLength = messageLength;
            _idDevice = 1;
        }


        public  byte[] GetAllRegisterMessage()
        {
            return PackageMessage(BedMessageEventType.ReadAll, 0 ,new byte[]{0x00, 0x80});
        }

        public BedRegisterValues SetAllRegisterValues(byte[] receiveMessage)
        {
            UnPackageMessage(receiveMessage);
            if (_messageData.Length < 255) throw new IndexOutOfRangeException();
            var values = new BedRegisterValues();
            //byte[] forFreqBytes = new byte[2];
           // _messageData.CopyTo(forFreqBytes,30);
           // values.Frequency = Half.ToHalf(forFreqBytes,0);  //todo проверка перевода во float - скорее всего запрос частоты будет не нужен
            values.BedStatus = (BedStatus)_messageData[BedStatusPosition];
            values.CurrentCycle = _messageData[CurrentCyclePosition];
            values.RemainingTime = TimeSpan.FromSeconds(GetValuesFromBytes(_messageData[RemainingTimePosition],_messageData[RemainingTimePosition + 1]));
            values.ElapsedTime =   TimeSpan.FromSeconds(GetValuesFromBytes(_messageData[ElapsedTimePosition],_messageData[ElapsedTimePosition + 1]));
            values.BedTargetAngleX = GetHalfValuesFromBytes(_messageData[BedTargetAngleXPosition],
                _messageData[BedTargetAngleXPosition + 1]);
            //todo здесь распаковываем пакет и заносим полученные значения

            return values;
        }
        
        /// <summary>
        /// создание пакета в виде массива байт
        /// </summary>
        /// <returns></returns>
        private byte[] PackageMessage(BedMessageEventType eventType, byte registerAddress, byte[] messageData)
        {
            byte[] message = new byte[10];
            message[0] = (byte)_startMessageMarker;
            message[1] = (byte) _idDevice;
            message[2] = (byte)eventType;
            message[3] = messageLength;
            message[5] = registerAddress; 
            message[6] = messageData[0];
            message[7] = messageData[1];
            byte[] messageForCRC = new byte[message.Length - 3];
            message.CopyTo(messageForCRC, 1);
            var crc = BedMessageCRC16.GetCRC16(messageForCRC);  //расчет контрольной суммы без маркера пакета
            message[8] = crc[0];
            message[9] = crc[1];
            return message;
        } 
        /// <summary>
        /// обработать полученное сообщение и извлечь из него данные
        /// </summary>
        /// <param name="inputMessage"></param>
        private void UnPackageMessage(byte[] inputMessage)
        {
            if (inputMessage == null || inputMessage.Length <= 4 ) throw new IndexOutOfRangeException("Пакет пуст или поврежден");
            
            if ((char) inputMessage[0] != '$')
            {
                throw new ArgumentException("Формат пакета неверен - не найден маркер начала пакета");
            }
            
            _idDevice = inputMessage[1];
            _eventType = (BedMessageEventType) inputMessage[2];
            messageLength = inputMessage[3];
            
            //ответный пакет это заголовок(1 байт) + ID (1) + тип (1) + размер данных в словах(1)
            //затем данные и 2 байта CRC16
            if (inputMessage.Length != 4 + messageLength * 2 + 2)  throw new ArgumentException("Неверный размер пакета");
            
            byte[] messageForCRC = new byte[inputMessage.Length - 3]; //CRC считаем для пакета кроме заголовка и самой суммы
            inputMessage.CopyTo(messageForCRC, 1);
            var crc = BedMessageCRC16.GetCRC16(messageForCRC);
            
            if (crc[0] != inputMessage[inputMessage.Length - 2] || crc[1] != inputMessage[inputMessage.Length - 1])
            {
                throw new ArgumentException("Неверная контрольная сумма");
            }
            
            _messageData = new byte[messageLength * 2];
            inputMessage.CopyTo(_messageData, 4);
            
           
            
        }

        private short GetValuesFromBytes(byte first, byte second)
        {
            return (short)(second + 256 * first);
        }
        
        private Half GetHalfValuesFromBytes(byte first, byte second)
        {
            return Half.ToHalf(new[] {first, second}, 0);
        }
    }
}
