using System;
using CardioMonitor.Devices.Monitor.Infrastructure;

namespace CardioMonitor.Devices.Monitor
{
    public class MitarMonitorDataParser
    {

        public Tuple<PatientCommonParams,PatientPressureParams> GetPatientCommonParams(byte[] message)
        {
            int iterator = 0;
            int startPacketIndex = 0;
            short heartRate = 0;
            short repsirationRate = 0;
            short spo2 = 0;

            short systolicArterialPressure = 0;
            short diastolicArterialPressure = 0;
            short averageArterialPressure = 0;
            //синхронизация
            bool isFindStartPacket = false;


            while (iterator < message.Length - 65 && !isFindStartPacket)
            {
                if (message[iterator] >> 4 == 0xE)
                {
                    byte[] forcrc = new byte[63];
                    Array.ConstrainedCopy(message, iterator, forcrc, 0, 63);
                    if (message[iterator + 63] == Crc8Calculator.GetCRC8(forcrc))
                    {
                        //все правильно - нашли пакет
                        startPacketIndex = iterator;
                        isFindStartPacket = true;
                    }
                }

                iterator++;
            }

            for (int i = startPacketIndex; i < message.Length - 65; i++)
            {
                byte[] forcrc = new byte[63];
                Array.ConstrainedCopy(message, i, forcrc, 0, 63);
                if (message[i + 63] == Crc8Calculator.GetCRC8(forcrc))
                {
                    //get IDX
                    int a = message[i + 2] >> 4;
                    int b = message[i + 4] >> 4;
                    int idx = a + (b << 4);
                    if (idx == 10)
                    {
                        int valueLow = message[i + 6] >> 4;
                        int valueHigh =
                            message[i + 8] >>
                            4; //todo работает только для чисел меньше 256 - там так то еще 2 куска параметров
                        repsirationRate = (short) (valueLow + (valueHigh << 4));
                    }

                    if (idx == 12)
                    {
                        int valueLow = message[i + 6] >> 4;
                        int valueHigh = message[i + 8] >> 4;
                        heartRate = (short) (valueLow + (valueHigh << 4));
                    }

                    if (idx == 13)
                    {
                        int valueLow = message[i + 6] >> 4;
                        int valueHigh = message[i + 8] >> 4;
                        spo2 = (short) (valueLow + (valueHigh << 4));
                    }

                    if (idx == 17)
                    {
                        int valueLow = message[i + 6] >> 4;
                        int valueHigh = message[i + 8] >> 4;
                        systolicArterialPressure = (short) (valueLow + (valueHigh << 4));
                    }

                    if (idx == 18)
                    {
                        int valueLow = message[i + 6] >> 4;
                        int valueHigh = message[i + 8] >> 4;
                        diastolicArterialPressure = (short) (valueLow + (valueHigh << 4));
                    }

                    if (idx == 19)
                    {
                        int valueLow = message[i + 6] >> 4;
                        int valueHigh = message[i + 8] >> 4;
                        averageArterialPressure = (short) (valueLow + (valueHigh << 4));
                    }

                    i += 64; //todo магические числа
                }
            } //todo проверка внутри пакета на crc + на попадение в допустимые рамки значений
           
           
           // byte[] forcrc = new byte[63];
            //Array.ConstrainedCopy(message, iterator, forcrc, 0, 63);

            //todo бла бла получили числа - дабавили в параметр



            return new Tuple<PatientCommonParams, PatientPressureParams>
                (new PatientCommonParams(heartRate, repsirationRate, spo2),
                new PatientPressureParams(systolicArterialPressure,diastolicArterialPressure,averageArterialPressure)); 
        }

        


    }
}