using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using CardioMonitor.Core.Models.Session;

namespace CardioMonitor.Core.Repository.Monitor
{
    class AsyncMonitorConnection
    {
        private static Socket serverSocket;
        private static object serverLock = new object();
        private class ConnectionInfo
        {
            public Socket Socket;
            public byte[] Buffer;
        }
        private static List<ConnectionInfo> connections =
           new List<ConnectionInfo>();
        public byte[] bufferBytes = new byte[4096];
        public int iterator = 0;
        public static void StartConnection()
        {
            IPAddress ipAddr = new IPAddress(new byte[] { 192, 168, 0, 147 });
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 4000);
            serverSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(ipEndPoint);
            serverSocket.Listen(10);                  //параметр  - макс число запросов в очереди
            //  Listener = sListener;

            //handler = sListener.Accept();
        }

        public static void Start()
        {
            try
            {
                StartConnection();
                for (int i = 0; i < 10; i++)
                {
                    serverSocket.BeginAccept(
                        new AsyncCallback(AcceptCallback), serverSocket);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("AsyncConnectionFail");
            }
        }
        private static void AcceptCallback(IAsyncResult result)
        {

            ConnectionInfo connection = new ConnectionInfo();
            try
            {
                // Finish Accept
                Socket s = (Socket)result.AsyncState;
                connection.Socket = s.EndAccept(result);
                connection.Socket.Blocking = false;
                connection.Buffer = new byte[255];
                lock (connections) connections.Add(connection);

                // Start Receive
                connection.Socket.BeginReceive(connection.Buffer, 0,
                    connection.Buffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), connection);
                // Start new Accept
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallback),
                    result.AsyncState);
            }
            catch (SocketException exc)
            {
                CloseConnection(connection);
                //Console.WriteLine("Socket exception: " + exc.SocketErrorCode);
            }
            catch (Exception exc)
            {
                CloseConnection(connection);
                // Console.WriteLine("Exception: " + exc);
            }
        }

        private static void ReceiveCallback(IAsyncResult result)
        {
            ConnectionInfo connection = (ConnectionInfo)result.AsyncState;

            try
            {
                int bytesRead = connection.Socket.EndReceive(result);
                if (0 != bytesRead)
                {
                    lock (serverLock)
                    {

                        /* if (showText)
                         {*/
                             string text = Encoding.UTF8.GetString(connection.Buffer, 0, bytesRead);
                             MessageBox.Show(text);
                             /*Console.Write(text);
                         }*/
                    }
                    lock (connections)
                    {
                        foreach (ConnectionInfo conn in connections)
                        {
                            if (connection != conn)
                            {
                                conn.Socket.Send(connection.Buffer, bytesRead,
                                    SocketFlags.None);
                            }
                        }
                    }
                    connection.Socket.BeginReceive(connection.Buffer, 0,
                        connection.Buffer.Length, SocketFlags.None,
                        new AsyncCallback(ReceiveCallback), connection);
                }
                else CloseConnection(connection);
            }
            catch (SocketException)
            {
                CloseConnection(connection);
            }
            catch (Exception)
            {
                CloseConnection(connection);
            }
        }

        private static void CloseConnection(ConnectionInfo ci)
        {
            ci.Socket.Close();
            lock (connections) connections.Remove(ci);
        }
    }
}
