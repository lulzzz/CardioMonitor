using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Devices.Bed.UDP
{
    /// <summary>
    /// вычисляем контрольную сумму методом (todo вспомнить каким)
    /// </summary>
    public class BedMessageCRC16
    {
        public static byte[] GetCRC16(byte[] inputMessage)
        {
            return null; //todo заглушка 
        }

        private ushort CRC16_Compute(byte data, ushort seed)
        {
            for (byte bitsLeft = 8; bitsLeft > 0; bitsLeft--) {
               byte temp = (byte)((seed ^ data) & 0x01);
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
