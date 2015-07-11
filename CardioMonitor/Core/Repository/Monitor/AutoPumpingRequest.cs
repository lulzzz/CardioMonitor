﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
//using CardioMonitor.Patients.Session;
using System.Windows;
using System.Windows.Forms;
using CardioMonitor.Core.Models.Session;

namespace CardioMonitor.Core.Repository.Monitor
{
    public class AutoPumpingRequest
    {
        public static bool StartAutoPumpingRequest()
        {
            //StartConnection();
            int outputData = 0;
           // PatientParams patientParams = new PatientParams();
            bool StopFlag = false;

            try
            {
                IPAddress ipAddr = new IPAddress(new byte[] { 192, 168, 0, 147 });
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 4000);
                Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sListener.Bind(ipEndPoint);
                sListener.Listen(1);

                Socket handler = sListener.Accept();
                   // byte[] sendMessage = new byte[25] { 0x70, 0x10, 0x50, 0x50, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x22 }; //автонакачко
                    byte[] sendMessage = new byte[25] { 0x70, 0x10, 0x50, 0x50, 0xaa, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa5 };   //до 170 мм
                    handler.Send(sendMessage);
                    Thread.Sleep(new TimeSpan(0, 0, 0, 1));
                    handler.Close();

                sListener.Close();
               // Thread.Sleep(new TimeSpan(0, 0, 0, 5 ));
               // Thread.Sleep(new TimeSpan(0, 0, 0, 50));
                return true;
            
               
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
