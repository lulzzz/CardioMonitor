//using CardioMonitor.Patients.Session;
using System;
using System.Net;
using System.Net.Sockets;
using CardioMonitor.Core.Models.Session;
using System.Threading.Tasks;

namespace CardioMonitor.Core.Repository.Monitor
{
    public class MonitorDataReader
    {
       
        //TODO зачем каждый раз создавать соединение?
        public static Task<PatientParams> GetPatientParams()
        {
            return Task.Factory.StartNew(() =>
            {
                //StartConnection();
                var patientParams = new PatientParams();
                var stopFlag = false;

                try
                {
                    var ipAddr = new IPAddress(new byte[] { 192, 168, 0, 147 });
                    var ipEndPoint = new IPEndPoint(ipAddr, 4000);
                    var sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    sListener.Bind(ipEndPoint);
                    sListener.Listen(10);


                    var handler = sListener.Accept();



                    // Начинаем слушать соединения
                    while (!stopFlag)
                    {


                        var bytes = new byte[6096];
                        var bytebuf = new byte[1024];
                        var iterator = 0;
                        var i = 0;
                        while (i < 6000)
                        {
                            int bytesRec = handler.Receive(bytebuf);
                            Array.ConstrainedCopy(bytebuf, 0, bytes, i, bytesRec);
                            i += bytesRec;
                        }
                        

                        {
                            bool stopSearchingFlag = false;

                            while (!stopSearchingFlag)
                            {
                                if ((bytes[iterator] >= 0xE0) && (bytes[iterator] <= 0xEF))
                                {
                                    if ((bytes[iterator + 2] >= 0xC0) && (bytes[iterator + 2] <= 0xCF) && (bytes[iterator + 4] <= 0x0F))       //поиск ЧСС
                                    {
                                        byte[] newbyte = new byte[24];
                                        Array.ConstrainedCopy(bytes, iterator, newbyte, 0, 24);
                                        if (CRC8.crc8Calculation(newbyte) == bytes[iterator + 24])
                                        {
                                            int Pulse1 = bytes[iterator + 6];
                                            int Pulse2 = bytes[iterator + 8];
                                            int Pulse3 = bytes[iterator + 10];
                                            int Pulse4 = bytes[iterator + 12];
                                            Pulse1 = Pulse1 >> 4;
                                            Pulse2 = Pulse2 >> 4;
                                            Pulse3 = Pulse3 >> 4;
                                            Pulse4 = Pulse4 >> 4;
                                            Pulse2 = Pulse2 << 4;
                                            Pulse3 = Pulse3 << 8;
                                            Pulse4 = Pulse4 << 12;
                                            int result = Pulse1 + Pulse2 + Pulse3 + Pulse4;

                                            if ((result >= 15) && (result <= 250))
                                            {
                                                patientParams.HeartRate = result;

                                            }
                                        }
                                    }
                                    if ((bytes[iterator + 2] >= 0xA0) && (bytes[iterator + 2] <= 0xAF) && (bytes[iterator + 4] <= 0x0F))          //поиск ЧД
                                    {
                                         byte[] newbyte = new byte[24];
                                        Array.ConstrainedCopy(bytes, iterator, newbyte, 0, 24);
                                        if (CRC8.crc8Calculation(newbyte) == bytes[iterator + 24])
                                        {
                                            int Pulse1 = bytes[iterator + 6];
                                            int Pulse2 = bytes[iterator + 8];
                                            int Pulse3 = bytes[iterator + 10];
                                            int Pulse4 = bytes[iterator + 12];
                                            Pulse1 = Pulse1 >> 4;
                                            Pulse2 = Pulse2 >> 4;
                                            Pulse3 = Pulse3 >> 4;
                                            Pulse4 = Pulse4 >> 4;
                                            Pulse2 = Pulse2 << 4;
                                            Pulse3 = Pulse3 << 8;
                                            Pulse4 = Pulse4 << 12;
                                            int result = Pulse1 + Pulse2 + Pulse3 + Pulse4;

                                            if ((result >= 0) && (result <= 180))   //ну или просто > 0
                                            {
                                                if ((result == 0) && (patientParams.RepsirationRate != 0))   //если по какой то причине возвращается ноль, хотя уже было получено ненулевое значение - проигнорировать ноль
                                                {

                                                }
                                                else        //да, я знаю что такой if/else это редкая лажа
                                                {
                                                    patientParams.RepsirationRate = result;
                                                }

                                            }
                                        }
                                    }
                                    if ((bytes[iterator + 2] >= 0xD0) && (bytes[iterator + 2] <= 0xDF) && (bytes[iterator + 4] <= 0x0F))          //поиск SPO2
                                    {
                                         byte[] newbyte = new byte[24];
                                        Array.ConstrainedCopy(bytes, iterator, newbyte, 0, 24);
                                        if (CRC8.crc8Calculation(newbyte) == bytes[iterator + 24])
                                        {
                                            int Pulse1 = bytes[iterator + 6];
                                            int Pulse2 = bytes[iterator + 8];
                                            int Pulse3 = bytes[iterator + 10];
                                            int Pulse4 = bytes[iterator + 12];
                                            Pulse1 = Pulse1 >> 4;
                                            Pulse2 = Pulse2 >> 4;
                                            Pulse3 = Pulse3 >> 4;
                                            Pulse4 = Pulse4 >> 4;
                                            Pulse2 = Pulse2 << 4;
                                            Pulse3 = Pulse3 << 8;
                                            Pulse4 = Pulse4 << 12;
                                            int result = Pulse1 + Pulse2 + Pulse3 + Pulse4;

                                            if ((result > 0) && (result <= 100))
                                            {
                                                patientParams.Spo2 = result;

                                            }
                                        }
                                    }
                                    if ((bytes[iterator + 2] >= 0x10) && (bytes[iterator + 2] <= 0x1F) && (bytes[iterator + 4] >= 0x10) && (bytes[iterator + 4] <= 0x1F)) //поиск АД среднее - на самом деле систолическое
                                    {
                                         byte[] newbyte = new byte[24];
                                        Array.ConstrainedCopy(bytes, iterator, newbyte, 0, 24);
                                        if (CRC8.crc8Calculation(newbyte) == bytes[iterator + 24])
                                        {
                                            int Pulse1 = bytes[iterator + 6];
                                            int Pulse2 = bytes[iterator + 8];
                                            int Pulse3 = bytes[iterator + 10];
                                            int Pulse4 = bytes[iterator + 12];
                                            Pulse1 = Pulse1 >> 4;
                                            Pulse2 = Pulse2 >> 4;
                                            Pulse3 = Pulse3 >> 4;
                                            Pulse4 = Pulse4 >> 4;
                                            Pulse2 = Pulse2 << 4;
                                            Pulse3 = Pulse3 << 8;
                                            Pulse4 = Pulse4 << 12;
                                            int result = Pulse1 + Pulse2 + Pulse3 + Pulse4;

                                            if ((result > 0) && (result < 250))
                                            {
                                                patientParams.SystolicArterialPressure = result;

                                            }
                                        }
                                    }
                                    if ((bytes[iterator + 4] >= 0x10) && (bytes[iterator + 4] <= 0x1F) && (bytes[iterator + 2] >= 0x20) && (bytes[iterator + 2] <= 0x2F)) //поиск АД диастол
                                    {
                                         byte[] newbyte = new byte[24];
                                        Array.ConstrainedCopy(bytes, iterator, newbyte, 0, 24);
                                        if (CRC8.crc8Calculation(newbyte) == bytes[iterator + 24])
                                        {
                                            int Pulse1 = bytes[iterator + 6];
                                            int Pulse2 = bytes[iterator + 8];
                                            int Pulse3 = bytes[iterator + 10];
                                            int Pulse4 = bytes[iterator + 12];
                                            Pulse1 = Pulse1 >> 4;
                                            Pulse2 = Pulse2 >> 4;
                                            Pulse3 = Pulse3 >> 4;
                                            Pulse4 = Pulse4 >> 4;
                                            Pulse2 = Pulse2 << 4;
                                            Pulse3 = Pulse3 << 8;
                                            Pulse4 = Pulse4 << 12;
                                            int result = Pulse1 + Pulse2 + Pulse3 + Pulse4;

                                            if ((result > 0) && (result <= 250))
                                            {
                                                patientParams.DiastolicArterialPressure = result;

                                            }
                                        }
                                    }
                                    if ((bytes[iterator + 4] >= 0x10) && (bytes[iterator + 4] <= 0x1F) && (bytes[iterator + 2] >= 0x30) && (bytes[iterator + 2] <= 0x3F)) //поиск АД среднее
                                    {
                                         byte[] newbyte = new byte[24];
                                        Array.ConstrainedCopy(bytes, iterator, newbyte, 0, 24);
                                        if (CRC8.crc8Calculation(newbyte) == bytes[iterator + 24])
                                        {
                                            int Pulse1 = bytes[iterator + 6];
                                            int Pulse2 = bytes[iterator + 8];
                                            int Pulse3 = bytes[iterator + 10];
                                            int Pulse4 = bytes[iterator + 12];
                                            Pulse1 = Pulse1 >> 4;
                                            Pulse2 = Pulse2 >> 4;
                                            Pulse3 = Pulse3 >> 4;
                                            Pulse4 = Pulse4 >> 4;
                                            Pulse2 = Pulse2 << 4;
                                            Pulse3 = Pulse3 << 8;
                                            Pulse4 = Pulse4 << 12;
                                            int result = Pulse1 + Pulse2 + Pulse3 + Pulse4;

                                            if ((result > 0) && (result <= 250))
                                            {
                                                patientParams.AverageArterialPressure = result;

                                            }
                                        }
                                    }
                                   /* if ((bytes[iterator + 2] >= 0x40) && (bytes[iterator + 2] <= 0x4F) && (bytes[iterator + 4] >= 0x10) && (bytes[iterator + 4] <= 0x1F))          //поиск давления в манжете
                                    {
                                        int Pulse1 = bytes[iterator + 6];
                                        int Pulse2 = bytes[iterator + 8];
                                        int Pulse3 = bytes[iterator + 10];
                                        int Pulse4 = bytes[iterator + 12];
                                        Pulse1 = Pulse1 >> 4;
                                        Pulse2 = Pulse2 >> 4;
                                        Pulse3 = Pulse3 >> 4;
                                        Pulse4 = Pulse4 >> 4;
                                        Pulse2 = Pulse2 << 4;
                                        Pulse3 = Pulse3 << 8;
                                        Pulse4 = Pulse4 << 12;
                                        int result = Pulse1 + Pulse2 + Pulse3 + Pulse4;

                                        if ((result >= 0) && (result <= 180))
                                        {
                                           // patientParams.RepsirationRate = result;
                                           

                                        }
                                    }*/
                                    // если все значения не нулевые (заполнились) - остановить поиск
                                    if ((patientParams.AverageArterialPressure != 0) &&
                                        (patientParams.DiastolicArterialPressure != 0) && (patientParams.HeartRate != 0) &&
                                        (patientParams.RepsirationRate != 0) && (patientParams.Spo2 != 0) &&
                                        (patientParams.SystolicArterialPressure != 0))
                                    {
                                        stopSearchingFlag = true;
                                        iterator = 0;
                                        stopFlag = true;
                                    }
                                }
                                if (iterator > (bytes.Length - 50))
                                {
                                    iterator = 0;
                                    stopSearchingFlag = true;
                                    stopFlag = true;
                                    break;
                                }
                                iterator++;

                            }
                            //  break;
                        }


                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                   // sListener.Shutdown(SocketShutdown.Both);
                    sListener.Close();                   
                    return patientParams;
                }
                catch (Exception ex)
                {
                    return patientParams;
                }
                
            });
            
        }
    }
}
