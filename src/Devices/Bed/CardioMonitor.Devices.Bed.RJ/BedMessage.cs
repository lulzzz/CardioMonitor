﻿using System;
using System.Linq;
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
        private byte _messageLength;

        /// <summary>
        /// Адрес регистра, из которого/ с которого/ в который производится чтение/запись
        /// </summary>
        private int _registerAddress;

        /// <summary>
        /// Данные из сообщения
        /// </summary>
        private byte[] _messageData;

        public BedMessage(){}
        
        public BedMessage(
            BedMessageEventType eventType,  
            byte messageLength = 2) //при запросе всех регистров - Length всегда 2 (адрес и дата по 1 слову)
        {
            _eventType = eventType;
            _messageLength = messageLength;
            _idDevice = 1;
        }

        /// <summary>
        /// Сообщение для установки блокировки кровати на время измерения показателей с КМ
        /// </summary>
        /// <param name="isBlock"> установить/снять блокировку </param>
        /// <returns></returns>
        public byte[] SetBedBlockMessage(bool isBlock)
        {
            return GetWriteRegisterMessage(
                BedBlockPosition, 
                isBlock 
                    ? new byte[] {0x00, 0x01} 
                    : new byte[] {0x00, 0x00});
        }

        
        
        public byte[] GetBedCommandMessage(BedControlCommand command) //todo а не слишком ли высокий уровень абстракции я сюда спустил?
        {
            switch (command)
            {
                case BedControlCommand.Start:
                {
                    return GetWriteRegisterMessage(BedMovingPosition, new byte[] {0x00, 0xAA});
                }
                case BedControlCommand.Pause:
                {
                    return GetWriteRegisterMessage(BedMovingPosition, new byte[] {0x00, 0xAA});
                }
                case BedControlCommand.EmergencyStop:
                {
                    return GetWriteRegisterMessage(BedMovingPosition, new byte[] {0x00, 0xAB});
                }
                case BedControlCommand.Reverse:
                {
                    return GetWriteRegisterMessage(BedMovingPosition, new byte[] {0x00, 0xAC});
                }
                default:
                {
                    throw new ArgumentException("Unknow type of command");
                }
            }
        }

        public byte[] SetFreqValueMessage(float freqValue)
        {
            var floatToByte = Half.GetBytes((Half) freqValue).Reverse().ToArray();
            return GetWriteRegisterMessage(BedFreqPosition, floatToByte);
        }

        public byte[] SetMaxAngleValueMessage(float maxAngleValue)
        {
            var floatToByte = Half.GetBytes((Half) maxAngleValue).Reverse().ToArray();
            return GetWriteRegisterMessage(BedMaxAnglePosition, floatToByte);
        }

        public byte[] SetCycleCountValueMessage(byte cycleCountValue)
        {
            return GetWriteRegisterMessage(BedCycleCountPosition, new byte[] {0x00, cycleCountValue});
        }

        private byte[] GetWriteRegisterMessage(byte regNum, byte[] messageToWrite)
        {
            return PackageMessage(BedMessageEventType.Write,regNum,messageToWrite);
        }

        public  byte[] GetAllRegisterMessage()
        {
            return PackageMessage(BedMessageEventType.ReadAll, 0 ,new byte[]{0x00, 0x80});
        }

        public BedRegisterValues GetAllRegisterValues(byte[] receiveMessage)
        {
            UnPackageMessage(receiveMessage);
            if (_messageData.Length < 255) throw new IndexOutOfRangeException();
            var values = new BedRegisterValues
            {
                BedStatus = GetBedStatus(_messageData[BedStatusPosition]),
                CurrentCycle = _messageData[CurrentCyclePosition],
                CurrentIteration = _messageData[CurrentIterationPosition],
                MaxAngle = GetHalfValuesFromBytes(_messageData[BedMaxAnglePosition * 2 + 1],
                _messageData[BedMaxAnglePosition * 2]),
                Frequency = GetHalfValuesFromBytes(_messageData[BedFreqPosition * 2 + 1],
                    _messageData[BedFreqPosition * 2]),
                RemainingTime = TimeSpan.FromSeconds(GetValuesFromBytes(_messageData[RemainingTimePosition],
                    _messageData[RemainingTimePosition + 1])),
                ElapsedTime = TimeSpan.FromSeconds(GetValuesFromBytes(_messageData[ElapsedTimePosition],
                    _messageData[ElapsedTimePosition + 1])),
                BedTargetAngleX = GetHalfValuesFromBytes(_messageData[BedTargetAngleXPosition + 1],
                    _messageData[BedTargetAngleXPosition])
            };
           

            //Здесь тоже измененный порядок байт - todo подумать как его красиво реверсить

            //values.BedR

            return values;
        }

        private BedStatus GetBedStatus(byte input)
        {
            switch (input)
            {
                case 0x01:
                {
                    return BedStatus.SessionStarted;
                }
                case 0x02:
                {
                    return BedStatus.EmergencyStop;
                }
                case 0x03:
                {
                    return BedStatus.Reverse;
                }
                case 0x04:
                {
                    return BedStatus.Pause;
                }
                case 0x00:
                {
                    return BedStatus.Ready;
                }
                default:
                {
                    return BedStatus.Unknown;
                }
            } 
        }
        
        /// <summary>
        /// создание пакета в виде массива байт
        /// </summary>
        /// <returns></returns>
        private byte[] PackageMessage(BedMessageEventType eventType, byte registerAddress, byte[] messageData)
        {
            var message = new byte[10];
            message[0] = (byte)_startMessageMarker;
            message[1] = _idDevice;
            message[2] = (byte)eventType;
            message[3] = _messageLength;
            message[5] = registerAddress; 
            message[6] = messageData[0];
            message[7] = messageData[1];
            var messageForCRC = new byte[message.Length - 2];

            for (var i = 0; i < messageForCRC.Length; i++)
            {
                messageForCRC[i] = message[i];
            }
            var crc = BitConverter.GetBytes(BedMessageCRC16.GetCRC16(messageForCRC));
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(crc);
            }
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
            _messageLength = inputMessage[3];
            if (_messageLength == 0) throw new ArgumentException("В пакете нет данных");

            //ответный пакет это заголовок(1 байт) + ID (1) + тип (1) + размер данных в словах(1)
            //затем данные и 2 байта CRC16
            if (inputMessage.Length != 4 + _messageLength * 2 + 2)  throw new ArgumentException("Неверный размер пакета");
            
            var messageForCRC = new byte[inputMessage.Length - 2]; //CRC считаем для пакета кроме самой суммы

            for (var i = 0; i < messageForCRC.Length; i++)
            {
                messageForCRC[i] = inputMessage[i];
            }
            var crcCalc = BedMessageCRC16.GetCRC16(messageForCRC);
            var crcReal =
                (ushort) (inputMessage[inputMessage.Length - 2] * 256 + inputMessage[inputMessage.Length - 1]);
            
            if (crcReal != crcCalc)
            {
                throw new ArgumentException("Неверная контрольная сумма");
            }
            
            _messageData = new byte[_messageLength * 2];

            if (inputMessage.Length < _messageData.Length + 4) throw new ArgumentException("Неверный размер пакета");

            for (var i = 0; i < _messageData.Length; i++)
            {
                _messageData[i] = inputMessage[i + 4];
            }
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
