using System;
using System.Threading.Tasks;
using MyHIDUSBLibrary;
using CardioMonitor.Core.Models.Connection;

namespace CardioMonitor.Core.Repository.Controller
{
    public class BedUSBConnection
    {
        public BedConnectionStatus Status;
        public BedMovingStatus MovingStatus;
        public bool _isConnection;
        public int flag_start = -1;
        public int flag_reverse = -1;
        public HIDDevice _device;

        /// <summary>
        /// Запрос текущего состояния готовности устройства (Неподключено, калибруется, готово, в работе(идет цикл), не готово(после аварийной остановки))
        /// </summary>
        public void GetConnectionStatus()
        {
            HIDDevice.interfaceDetails[] devices = HIDDevice.getConnectedDevices();
            string devicePath = null;
            foreach (var listofDevice in devices)
            {
                if (listofDevice.product == "belmed_v1")
                {
                    devicePath = listofDevice.devicePath;
                    break;
                }
            }
            if (devicePath == null)
            {
                Status = BedConnectionStatus.UnConnected;
            }
            else
            {
                HIDDevice device = new HIDDevice(devicePath, false);
                var message = new byte[] {0x6e, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};
                device.write(message);
                byte[] readData = device.read();
                if (readData != null)
                {
                    if (readData[3] == 0)
                    {
                        Status = BedConnectionStatus.Calibrating;
                    }
                    if (readData[3] == 1)
                    {
                        Status = BedConnectionStatus.Ready;
                    }
                    if (readData[3] == 2)
                    {
                        Status = BedConnectionStatus.Loop;
                    }
                    if (readData[3] == 3)
                    {
                        Status = BedConnectionStatus.NotReady;
                    } //если почему то придет другое значение - поле останется пустым! (add Unknow?)
                }
                device.close();
            }
        }

        /// <summary>
        /// Запрос текущего статуса устройства (готовность к старту, подготовка к движению наверх, двжиение наверх, подготовка к движению вниз, движение вниз, подготовка (после аварийной остановки), нет соединения)
        /// </summary>
        public void GetBedMovingStatus()
        {
            HIDDevice.interfaceDetails[] devices = HIDDevice.getConnectedDevices();
            string devicePath = null;
            foreach (var listofDevice in devices)
            {
                if (listofDevice.product == "belmed_v1")
                {
                    devicePath = listofDevice.devicePath;
                    break;
                }
            }
            if (devicePath == null)
            {
                MovingStatus = BedMovingStatus.UnConnected;

            }
            else
            {
                HIDDevice device = new HIDDevice(devicePath, false);
                var message = new byte[] {0x6e, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};
                device.write(message);
                byte[] readData = device.read();
                if (readData != null)
                {
                    if (readData[4] == 13)
                    {
                        MovingStatus = BedMovingStatus.Ready;
                    }
                    if ((readData[4] == 14) || (readData[4] == 15))
                    {
                        MovingStatus = BedMovingStatus.PrepareToUpMoving;
                    }
                    if (readData[4] == 16)
                    {
                        MovingStatus = BedMovingStatus.UpMoving;
                    }
                    if (readData[4] == 17)
                    {
                        MovingStatus = BedMovingStatus.PrepareToDownMoving;
                    }
                    if (readData[4] == 18)
                    {
                        MovingStatus = BedMovingStatus.DownMoving;
                    }
                    if (readData[4] == 33)
                    {
                        MovingStatus = BedMovingStatus.WaitingAfterEmergency;
                    }
                }
                device.close();
            }
        }

        /// <summary>
        /// Запрос флага старт/пауза (0 - пауза, 1- старт, -1 - изначальное состояние) и флага реверса (0 - реверс не вызван, 1 - вызван, -1 - изначальное состояние)
        /// </summary>
        public void GetFlags()
        {
            HIDDevice.interfaceDetails[] devices = HIDDevice.getConnectedDevices();
            string devicePath = null;
            foreach (var listofDevice in devices)
            {
                if (listofDevice.product == "belmed_v1")
                {
                    devicePath = listofDevice.devicePath;
                    break;
                }
            }
            if (devicePath != null)
            {
                HIDDevice device = new HIDDevice(devicePath, false);
                var message = new byte[] {0x6e, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};
                device.write(message);
                byte[] readData = device.read();
                if (readData != null)
                {
                    if (readData[5] == 0)
                    {
                        flag_start = 0;
                    }
                    if (readData[5] == 1)
                    {
                        flag_start = 1;
                    }
                    if (readData[7] == 0)
                    {
                        flag_reverse = 0;
                    }
                    if (readData[7] == 1)
                    {
                        flag_reverse = 1;
                    }
                }
                device.close();
            }
        }

        /// <summary>
        /// Угол по оси Х
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static Task<double> GetAngleXAsync()
        {
            //TODO Вот и все, эта задача будет выполняться в отдельном потоке, взятом из
            //TODO пула потоков, UI поток не блокируется.
            return Task.Factory.StartNew(() =>
            {
                HIDDevice device = GetDevice();
                if (device != null)
                {
                    var message = new byte[] { 0x50, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff };
                    device.write(message);
                    byte[] readData = device.read();
                    int lowbyte = 0;
                    int highbyte = 0;
                    int replyX = 0; //первоначальное кол-во отчетов для оси Х
                    if (readData != null)
                    {
                        lowbyte = readData[3];
                        highbyte = readData[4];
                        highbyte = highbyte << 8;
                        replyX = highbyte + lowbyte;
                    }
                    message = new byte[] { 0x52, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff };
                    device.write(message);
                    readData = device.read();
                    lowbyte = 0;
                    highbyte = 0;
                    int factorX = 0;
                    //множитель, на который делится кол-во отчетов, параметр прописан в прошивке и зависит от железа
                    if (readData != null)
                    {
                        lowbyte = readData[3];
                        highbyte = readData[4];
                        highbyte = highbyte << 8;
                        factorX = highbyte + lowbyte;

                    }
                    message = new byte[] { 0x54, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff };
                    device.write(message);
                    readData = device.read();
                    lowbyte = 0;
                    highbyte = 0;
                    int initialValueX = 0; // начальное значение, вычитается из кол-ва отчетов
                    if (readData != null)
                    {
                        lowbyte = readData[3];
                        highbyte = readData[4];
                        highbyte = highbyte << 8;
                        initialValueX = highbyte + lowbyte;

                    }
                    double resultX = 0;
                    if (factorX != 0)
                    {
                        double mnX = factorX / 1.5;
                        resultX = (replyX - initialValueX) / mnX;
                        resultX = Math.Round(resultX, 2);
                    }
                    device.close();
                    return resultX;                   
                }
                return 0;
            });
        }

        /// <summary>
        /// Угол по оси Y
        /// </summary>
        public static double GetAngleY()
        {
            HIDDevice device = GetDevice();
            if (device != null)
            {
                var message = new byte[] {0x51, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};
                device.write(message);
                byte[] readData = device.read();
                int lowbyteY = 0;
                int highbyteY = 0;
                int replyY = 0; //первоначальное кол-во отчетов для оси Y
                if (readData != null)
                {
                    lowbyteY = readData[3];
                    highbyteY = readData[4];
                    highbyteY = highbyteY << 8;
                    replyY = highbyteY + lowbyteY;
                }
                message = new byte[] {0x53, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};
                device.write(message);
                readData = device.read();
                lowbyteY = 0;
                highbyteY = 0;
                int factorY = 0;
                    //множитель, на который делится кол-во отчетов, параметр прописан в прошивке и зависит от железа
                if (readData != null)
                {
                    lowbyteY = readData[3];
                    highbyteY = readData[4];
                    highbyteY = highbyteY << 8;
                    factorY = highbyteY + lowbyteY;
                }
                message = new byte[] {0x55, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};
                device.write(message);
                readData = device.read();
                lowbyteY = 0;
                highbyteY = 0;
                int initialValueY = 0; // начальное значение, вычитается из кол-ва отчетов
                if (readData != null)
                {
                    lowbyteY = readData[3];
                    highbyteY = readData[4];
                    highbyteY = highbyteY << 8;
                    initialValueY = highbyteY + lowbyteY;

                }
                double resultY = 0;
                if (factorY != 0)
                {
                    double mnY = factorY/3.0;
                    resultY = (replyY - initialValueY)/mnY;

                }
                device.close();
                return resultY;
            }
            else
            {
                return 0;
            }

        }

        /// <summary>
        /// получение устройства (пути к нему для дальнейшей работы с ним)
        /// </summary>
        public static HIDDevice GetDevice()
        {
            HIDDevice.interfaceDetails[] devices = HIDDevice.getConnectedDevices();
            string devicePath = null;
            HIDDevice device;
            foreach (var listofDevice in devices)
            {
                if (listofDevice.product == "belmed_v1") //итоговое название всех прошивок - другое
                {
                    devicePath = listofDevice.devicePath;
                    break;
                }
            }
            if (devicePath != null)
            {
                device = new HIDDevice(devicePath, false);
            }
            else
            {
                device = null;
            }
            return device;


        }

        /// <summary>
        /// подключено ли устройство
        /// </summary>
        /// <returns></returns>
        public bool IsConnecting()
        {
            HIDDevice.interfaceDetails[] devices = HIDDevice.getConnectedDevices();
            string devicePath = null;
            foreach (var listofDevice in devices)
            {
                if (listofDevice.product == "belmed_v1")
                {
                    devicePath = listofDevice.devicePath;
                    break;
                }
            }
            if (devicePath != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void StartCommand(string commandName)
        {
            try
            {
                HIDDevice device = GetDevice();
                byte[] message = null;
                if (device != null)
                {
                    switch (commandName)
                    {
                        case "Start":
                            message = new byte[] { 0x29, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff };

                            break;
                        case "Pause":
                            message = new byte[] { 0x2a, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff };

                            break;
                        case "Reverse":
                            message = new byte[] { 0x2c, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff };

                            break;
                        case "EmergencyStop":
                            message = new byte[] { 0x2b, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff };

                            break;
                    }
                    device.write(message);
                    device.close();
                }
            }
            catch (Exception ex)
            {
                //error message
            }
        }
    }
}
