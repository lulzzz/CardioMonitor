using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Devices.Bed.UDP
{
    public class BedMessage
    {
        /// <summary>
        /// Маркер - идентификатор начала пакета
        /// </summary>
        private char startMessageMarker = '$';

        /// <summary>
        /// устройство, с которого пришла информация 
        /// </summary>
        private BedMessageDeviceNumber idDevice;  //todo на текущий момент малофункционально, ибо работаем только с одной кроватью

        /// <summary>
        /// тип события: информация читается или записывается
        /// </summary>
        private BedMessageEventType eventType;

        /// <summary>
        /// размер пакета данных
        /// </summary>
        private byte messageLength;

        /// <summary>
        /// Адрес регистра, из которого/в который производится чтение/запись
        /// </summary>
        private int registerAddress;

        /// <summary>
        /// Данные из сообщения
        /// </summary>
        private byte[] messageData;


        public BedMessage(BedMessageEventType eventType, int registerAddress, byte[] messageData)
        {
            this.eventType = eventType;
            this.messageLength = (byte)messageData.Length; //todo нуждается в уточнении, что это за размер пакета, по протоколу он всегда равен 2 байта
            this.registerAddress = registerAddress;
            this.messageData = messageData;
        }

        /// <summary>
        /// создание пакета в виде массива байт
        /// </summary>
        /// <returns></returns>
        public byte[] PackageMessage()
        {
            byte[] message = new byte[10];
            message[0] = (byte)startMessageMarker;
            message[1] = (byte) idDevice;
            message[2] = (byte)eventType;
            message[3] = messageLength;
            message[4] = (byte)registerAddress; //регистр по спецификации 2 байтный, уточнить тип нотации при записи
            message[6] = messageData[0];
            message[7] = messageData[1];
            byte[] messageForCRC = new byte[7];
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
        public void UnPackageMessage(byte[] inputMessage)
        {
            if ((char) inputMessage[0] != '$')
            {
                throw new ArgumentException("Формат пакета неверен - не найден маркер начала пакета");
            }
            idDevice = (BedMessageDeviceNumber)inputMessage[1];
            eventType = (BedMessageEventType) inputMessage[2];
            messageLength = inputMessage[3];
            registerAddress = inputMessage[4];
            messageData[0] = inputMessage[6];
            messageData[1] = inputMessage[7];
            byte[] messageForCRC = new byte[7];
            inputMessage.CopyTo(messageForCRC, 1);
            var crc = BedMessageCRC16.GetCRC16(messageForCRC);
            if (crc[0] != inputMessage[8] && crc[1] != inputMessage[9])
            {
                throw new ArgumentException("Неверная контрольная сумма");
            }
        }
    }
}
