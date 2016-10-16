using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

//using CardioMonitor.Patients.Session;

namespace CardioMonitor.Devices.Monitor
{
    public class AutoPumpingRequest
    {
        private readonly static object _lockObject = new object();

        public static bool StartAutoPumpingRequest()
        {
            //StartConnection();
            int outputData = 0;
           // PatientParams patientParams = new PatientParams();
            bool StopFlag = false;
           // lock (_lockObject)
            {
                try
                {
                    IPAddress ipAddr = new IPAddress(new byte[] { 192, 168, 0, 147 });
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 4000);
                    Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    sListener.Bind(ipEndPoint);
                    sListener.Listen(10);

                    Socket handler = sListener.Accept();
                     byte[] sendMessage = new byte[25] { 0x70, 0x10, 0x50, 0x50, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x22 }; //автонакачко
                   // byte[] sendMessage = new byte[25] { 0x70, 0x10, 0x50, 0x50, 0xaa, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa5 };   //до 170 мм
                    handler.Send(sendMessage);
                    Thread.Sleep(new TimeSpan(0, 0, 0, 2));
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                  //  sListener.Shutdown(SocketShutdown.Both);
                    sListener.Close();
                    return true;


                }
                catch (TimeoutException)
                {
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}

