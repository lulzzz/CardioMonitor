﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
//using CardioMonitor.Patients.Session;
using System.Windows;
using System.Windows.Forms;
using CardioMonitor.Core.Models.Session;

namespace CardioMonitor.MonitorConnection
{
    public class MonitorConnection
    {
        public static Socket Listener;
        public static void StartConnection()
        {
            IPAddress ipAddr = new IPAddress(new byte[] { 192, 168, 0, 147 });
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 4000);
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sListener.Bind(ipEndPoint);
            sListener.Listen(10);
            Listener = sListener;

            //handler = sListener.Accept();
        }
        public static PatientParams StartTCPConnection(Socket Listner)
        {
            //StartConnection();
            int outputData = 0;
            PatientParams patientParams = new PatientParams();
            bool StopFlag = false;

            try
            {
                /*IPAddress ipAddr = new IPAddress(new byte[] { 192, 168, 0, 147 });
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 4000);
                Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sListener.Bind(ipEndPoint);
                sListener.Listen(10)*/

                //Listener = sListener; 
                //Listner.
                /* Socket handler1 = Listener.Accept();              //КАСТЫЛЬ!
                 byte[] bytesx = new byte[1024];                   //По невыясненной причине
                 int bytesRec1451 = handler1.Receive(bytesx);      //Первый запрос после ожидания
                 //handler1.Shutdown(SocketShutdown.Both);           //Выдает старые данные
                 handler1.Close();      */
                //Фейковый запрос к слушателю
                Socket handler = Listner.Accept();

                // List<byte[]> bufer = new List<byte[]>();

                // Начинаем слушать соединения
                while (!StopFlag)
                {
                    // MessageBox.Show("Ожидаем соединение через порт", "ipEndPoint.ToString()");

                    // Программа приостанавливается, ожидая входящее соединение
                    //Socket handler = sListener.Accept();
                    // string data = null;

                    // Мы дождались клиента, пытающегося с нами соединиться

                    byte[] bytes = new byte[4096];
                    byte[] bytebuf = new byte[1024];
                    int bytesRec;
                    int iterator = 0;
                    int i = 0;
                    while (i < 4000)
                    {
                        bytesRec = handler.Receive(bytebuf);
                        Array.ConstrainedCopy(bytebuf, 0, bytes, i, bytesRec);
                        i += bytesRec;
                    }

                    //  bytesRec = handler.Receive(bytes);

                    // MessageBox.Show(Convert.ToString(bytesRec));
                    //i++;
                    //  while ((bytesRec = handler.Receive(bytes)) > 0)
                    // if (bytesRec > 1000)
                    {
                        bool stopSearchingFlag = false;
                        //bool stopSearchingFlag = false;
                        //int bytesRec = handler.Receive(bytes);
                        //bufer.Add(bytes);
                        while (!stopSearchingFlag)
                        {
                            if ((bytes[iterator] >= 0xE0) && (bytes[iterator] <= 0xEF))
                            {
                                if ((bytes[iterator + 2] >= 0xC0) && (bytes[iterator + 2] <= 0xCF) && (bytes[iterator + 4] <= 0x0F))       //поиск ЧСС
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
                                        // outputData = result;
                                        // MessageBox.Show(Convert.ToString(i));
                                        //stopSearchingFlag = true;
                                        //iterator = 0;
                                        //StopFlag = true;
                                        //return outputData;
                                    }
                                }
                                if ((bytes[iterator + 2] >= 0xA0) && (bytes[iterator + 2] <= 0xAF) && (bytes[iterator + 4] <= 0x0F))          //поиск ЧД
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
                                        patientParams.RepsirationRate = result;
                                        // MessageBox.Show(Convert.ToString(i));
                                        //stopSearchingFlag = true;
                                        //iterator = 0;
                                        //StopFlag = true;
                                        //return outputData;
                                    }
                                }
                                if ((bytes[iterator + 2] >= 0xD0) && (bytes[iterator + 2] <= 0xDF) && (bytes[iterator + 4] <= 0x0F))          //поиск SPO2
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
                                        // MessageBox.Show(Convert.ToString(i));
                                        //stopSearchingFlag = true;
                                        //iterator = 0;
                                        //StopFlag = true;
                                        //return outputData;
                                    }
                                }
                                if ((bytes[iterator + 2] >= 0x10) && (bytes[iterator + 2] <= 0x1F) && (bytes[iterator + 4] >= 0x10) && (bytes[iterator + 4] <= 0x1F)) //поиск АД среднее - на самом деле систолическое
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

                                    if ((result > 0) && (result < 300))
                                    {
                                        patientParams.SystolicArterialPressure = result;
                                        // outputData = result;
                                        // MessageBox.Show(Convert.ToString(i));
                                        //stopSearchingFlag = true;
                                        //iterator = 0;
                                        //StopFlag = true;
                                        //return outputData;
                                    }
                                }
                                if ((bytes[iterator + 4] >= 0x10) && (bytes[iterator + 4] <= 0x1F) && (bytes[iterator + 2] >= 0x20) && (bytes[iterator + 2] <= 0x2F)) //поиск АД диастол
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

                                    if ((result >= 0) && (result <= 300))
                                    {
                                        patientParams.DiastolicArterialPressure = result;
                                        // outputData = result;
                                        // MessageBox.Show(Convert.ToString(i));
                                        //stopSearchingFlag = true;
                                        //iterator = 0;
                                        //StopFlag = true;
                                        //return outputData;
                                    }
                                }
                                if ((bytes[iterator + 4] >= 0x10) && (bytes[iterator + 4] <= 0x1F) && (bytes[iterator + 2] >= 0x30) && (bytes[iterator + 2] <= 0x3F)) //поиск АД сист
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

                                    if ((result > 0) && (result <= 300))
                                    {
                                        patientParams.AverageArterialPressure = result;
                                        // outputData = result;
                                        // MessageBox.Show(Convert.ToString(i));
                                        //stopSearchingFlag = true;
                                        //iterator = 0;
                                        //StopFlag = true;
                                        //return outputData;
                                    }
                                } // если все значения не нулевые (заполнились) - остановить поиск
                                if ((patientParams.AverageArterialPressure != 0) &&
                                    (patientParams.DiastolicArterialPressure != 0) && (patientParams.HeartRate != 0) &&
                                    (patientParams.RepsirationRate != 0) && (patientParams.Spo2 != 0) &&
                                    (patientParams.SystolicArterialPressure != 0))
                                {
                                    stopSearchingFlag = true;
                                    iterator = 0;
                                    StopFlag = true;
                                }
                            }
                            if (iterator > (bytes.Length - 24))
                            {
                                iterator = 0;
                                stopSearchingFlag = true;
                                StopFlag = true;
                                break;
                            }
                            iterator++;

                        }
                        //  break;
                    }


                }

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                //sListener.Shutdown(SocketShutdown.Both);
                //sListener.Close();
                //Listener.Dispose();
                // patientParams.InclinationAngle = 10.5;
                return patientParams;
            }
            catch (Exception ex)
            {
                return patientParams;
            }
        }
    }
}