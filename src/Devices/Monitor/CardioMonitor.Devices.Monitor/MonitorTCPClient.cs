using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace CardioMonitor.Devices.Monitor
{
    [Obsolete("Нигде не используется.")]
    public class MonitorTCPClient
    {
        public delegate void ReceiveDataCallback();

        public delegate void DisconnectedCallback();

        public delegate void ErrorCallback(); //todo обдумать необходимость

        /// <summary>
        /// 
        /// </summary>
        public ReceiveDataCallback OnReceiveData { get; set; }
        
        public DisconnectedCallback OnDisconnected { get; set; }

        public Queue<byte[]> InputMessageQueue { get; set; }

        private TcpClient _tcpClient;

        public void Connect(IPEndPoint ipAdress)
        {
            try
            {
                _tcpClient.Connect(ipAdress);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void DisConnect()
        {
            try
            {
                _tcpClient?.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void WaitForData()
        {
            try
            {
                var ns = _tcpClient.GetStream();
//                byte[] inputMessage = new byte[];
//                ns.BeginRead()
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
          
        
            UdpClient udpCLient;
            public delegate void InvokeDelegate();
            private int[] ECGData = new int[1125];//типа за 3 секунды
            
            public void TestTcpConnection(IPEndPoint ipendpoint)
            {
                try
                {
                    TcpClient tcpClient = new TcpClient();
                    tcpClient.Connect(ipendpoint.Address, 9761);
                    NetworkStream ns = tcpClient.GetStream();
                    int i = 0;
                    while (ns.DataAvailable && i < ECGData.Length)
                    {
                        byte[] text = new byte[1024];
                        var buffSize = ns.Read(text, 0, text.Length);
                        if (i + buffSize < ECGData.Length)
                        {
                            Array.ConstrainedCopy(text, 0, ECGData, i, buffSize);

                        }
                        i += buffSize;
                        //var data = GetECGValue(text);
                        //ECGData[i] = data[0];
                        //ECGData[i + 1] = data[1];
                        //i+=2;
                    }
                    
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
//                        MessageBox.Show("Разрыв соединения");
                    }
                }
                catch (Exception ex)
                {
//                    MessageBox.Show(ex.Message);
                }
            }
            /// <summary>
            /// тест на накачку давления
            /// </summary>
            /// <param name="ns"></param>
            public void SendMessageToCM(NetworkStream ns)
            {
                byte[] sendMessage = new byte[25]
                {
                0x70, 0x10, 0x50, 0x50, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x22
                };
                ns.Write(sendMessage, 0, sendMessage.Length);
            }
            public void TestTest(IPEndPoint ipendpoint)
            {
                try
                {
                    var bytes = new byte[6144];
                    var iterator = 0;
                    TcpClient tcpClient = new TcpClient();
                    tcpClient.Connect(ipendpoint.Address, 9761);
                    NetworkStream ns = tcpClient.GetStream();
                    SendMessageToCM(ns);
                    while (true)
                    {
                        int i = 0;
                        while (i < 6000)
                        {
                            byte[] text = new byte[1024];
                            int buffSize = ns.Read(text, 0, text.Length);
                            //richTextBox1.BeginInvoke(new InvokeDelegate(() => { richTextBox1.Text += buffSize + "\n"; }));
                            Array.ConstrainedCopy(text, 0, bytes, i, buffSize);
                            i += buffSize;
                        }
                        TestParamNumber(bytes);
                    }
                    // richTextBox1.BeginInvoke(new InvokeDelegate(() => { richTextBox1.Text += ""; }));
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
//                        MessageBox.Show("Разрыв соединения");
                    }
                }
                catch (Exception ex)
                {
//                    MessageBox.Show(ex.Message);
                }


            }

           
            public void TcpConnectionForPump(IPEndPoint ipendpoint)
            {
                try
                {
                    TcpClient tcpClient = new TcpClient();
                    tcpClient.Connect(ipendpoint.Address, 9761);
                    NetworkStream ns = tcpClient.GetStream();
                    int i = 0;
                    while (ns.DataAvailable)
                    {
                        // var a = ns.Length.ToString();
                        byte[] text = new byte[1024];
                        ns.Read(text, 0, text.Length);
                        // if (CRC8.crc8Calculation(text) == text[63])
                        {

                            if (GetIndexConfig(text[2], text[4]) == 22) //байт статуса модуля
                            {
                                int valueLow = text[6] >> 4;
                                int valueHigh = text[8] >> 4;
                                var code = valueLow + (valueHigh << 4);
//                                label1.Text = code.ToString();
                            }

                        }
                    }
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
//                        MessageBox.Show("Разрыв соединения");
                    }
                }
                catch (Exception ex)
                {
//                    MessageBox.Show(ex.Message);
                }
            }

            public void TestParamNumber(byte[] inputArray)
            {
                int iterator = 0;
                List<int> numbersList = new List<int>();
                while (iterator < inputArray.Length - 1100)
                {
                    if (inputArray[iterator] >> 4 == 0x0E)
                    {
                        byte[] forcrc = new byte[63];
                        Array.ConstrainedCopy(inputArray, iterator, forcrc, 0, 63);
//                        byte calccrc = CRC8.crc8Calculation(forcrc);
                        byte realcrc = inputArray[iterator + 63];
                        int a = inputArray[iterator + 2] >> 4;
                        int b = inputArray[iterator + 4] >> 4;
                        int c = a + (b << 4);
                        if (c == 20)
                        {
                            int valueLow = inputArray[iterator + 6] >> 4;
                            int valueHigh = inputArray[iterator + 8] >> 4;
                            int code = valueLow + (valueHigh << 4);
//                            richTextBox1.BeginInvoke(new InvokeDelegate(() => { richTextBox1.Text += code + "  - давление в манжете" + "\n"; }));
                        }
                        if (c == 22)
                        {
                            int valueLow = inputArray[iterator + 6] >> 4;
                            int valueHigh = inputArray[iterator + 8] >> 4;
                            var code = valueLow + (valueHigh << 4);
//                            richTextBox1.BeginInvoke(new InvokeDelegate(() => { richTextBox1.Text += (CMStatus)code + "\n"; }));
                        }
                        numbersList.Add(c);
                    }
                    iterator += 64;
                }
            }

            public int GetIndexConfig(byte firstByte, byte secondByte)
            {
                var cfg1 = firstByte >> 4;
                var cfg2 = secondByte >> 4;
                return cfg1 + (cfg2 << 4);
            }
            public void Receive() //прием входящего бродкаст Udp пакета
            {
                udpCLient = new UdpClient(30304);
                while (true)
                {
                    IPEndPoint ipendpoint = null;
                    byte[] message = udpCLient.Receive(ref ipendpoint);
                    // TestTcpConnection(ipendpoint);
                    //TcpConnectionForPump(ipendpoint);
                    TestTest(ipendpoint);
                }
            }

            private void startTestButton_Click(object sender, EventArgs e)
            {
                // TestTcpConnection(new IPEndPoint(IPAddress.Parse("192.168.0.204"),2860));
//                Thread newThread = new Thread(Receive);
//                newThread.Start();
            }

            public int[] GetECGValue(byte[] inputArray)
            {

                int iterator = 0;
                List<int> numbersList = new List<int>();
                while (iterator < inputArray.Length - 1100)
                {
                    if (inputArray[iterator] >> 4 == 0x0E)
                    {
                        byte[] forcrc = new byte[63];
                        Array.ConstrainedCopy(inputArray, iterator, forcrc, 0, 63);
//                        byte calccrc = CRC8.crc8Calculation(forcrc);
                        byte realcrc = inputArray[iterator + 63];
                        int a = inputArray[iterator + 2] >> 4;
                        int b = inputArray[iterator + 4] >> 4;
                        int c = a + (b << 4);
                        if (c == 20)
                        {
                            int valueLow = inputArray[iterator + 6] >> 4;
                            int valueHigh = inputArray[iterator + 8] >> 4;
                            var code = valueLow + (valueHigh << 4);
//                            richTextBox1.BeginInvoke(new InvokeDelegate(() => { richTextBox1.Text += code + "  - давление в манжете" + "\n"; }));
                        }
                        if (c == 22)
                        {
                            int valueLow = inputArray[iterator + 6] >> 4;
                            int valueHigh = inputArray[iterator + 8] >> 4;
                            var code = valueLow + (valueHigh << 4);
//                            richTextBox1.BeginInvoke(new InvokeDelegate(() => { richTextBox1.Text += (CMStatus)code + "\n"; }));
                        }
                        numbersList.Add(c);
                    }
                    iterator += 64;
                }


                var first = inputArray[0] & 0x0F;
                first = first * 256 + inputArray[1];
                var second = inputArray[17] & 0x0F;
                second = second * 256 + inputArray[18];
                var answer = new[] { first, second };
                return answer;
            }

    }
}
