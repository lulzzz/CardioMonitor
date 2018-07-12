using System;

namespace CardioMonitor.Devices.Bed.UDP
{
    /// <summary>
    /// вычисляем контрольную сумму 
    /// </summary>
    public class BedMessageCRC16
    {
        public static ushort GetCRC16(byte[] inputMessage)
        {
            if (inputMessage == null) throw new ArgumentException();

            ushort crc = 0xffff;

            for (var i = 0; i < inputMessage.Length; i++)
            {
                crc = Compute(inputMessage[i], crc);
            }
            return crc; 
        }

        private static ushort Compute(byte data, ushort seed)
        {
            for (byte bitsLeft = 8; bitsLeft > 0; bitsLeft--) {
               var temp = (byte)((seed ^ data) & 0x01);
                if (temp == 0) {
                    seed >>= 1;
                } else {
                    seed ^= 0x4002;
                    seed >>= 1;
                    seed |= 0x8000;
                }
                data >>= 1;
            }
            return seed;  
        }
    }
}
