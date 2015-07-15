using System;
using System.Threading.Tasks;
using MyHIDUSBLibrary;

namespace CardioMonitor.Core.Repository.BedController
{
    /// <summary>
    /// Контроллер для взаимодействия с кроватью через Usb соединение
    /// </summary>
    /// <remarks>
    /// Потокобезопасный. И должен оставаться таким. 
    /// Необходимо сделать все методы асинхронными.
    /// </remarks>
    public class BedUsbController
    {
        private readonly object _lockObject;

        public StartFlag StartFlag { get; set; }
        public ReverseFlag ReverseFlag { get; set; }

        public BedUsbController()
        {
            StartFlag = StartFlag.Default; 
            ReverseFlag = ReverseFlag.Default;
            _lockObject = new object();
        }

        /// <summary>
        /// Запрос текущего состояния устройства
        /// </summary>
        public BedStatus GetBedStatus()
        {
            try
            {
                var device = GetBedUsbDevice();
                if (device == null) { return BedStatus.Disconnected;}

                lock (_lockObject)
                {
                    var message = new byte[] {0x6e, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};
                    device.write(message);

                    var status = BedStatus.Unknown;
                    var readData = device.read();
                    if (readData != null)
                    {
                        if (readData[3] == 0)
                        {
                            status = BedStatus.Calibrating;
                        }
                        if (readData[3] == 1)
                        {
                            status = BedStatus.Ready;
                        }
                        if (readData[3] == 2)
                        {
                            status = BedStatus.Loop;
                        }
                        if (readData[3] == 3)
                        {
                            status = BedStatus.NotReady;
                        }
                    }
                    device.close();
                    return status;
                }
            }
            catch (Exception ex)
            {
                return BedStatus.Unknown;
            }
        }

        /// <summary>
        /// Запрос текущего статуса движения кровати устройства 
        /// </summary>
        public BedMovingStatus GetBedMovingStatus()
        {
            var device = GetBedUsbDevice();
            if (device == null) { return BedMovingStatus.Disconnected; }

            lock (_lockObject)
            {
                var bedMovingStatus = BedMovingStatus.Disconnected;

                var message = new byte[] {0x6e, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};
                device.write(message);
                var readData = device.read();
                if (readData != null)
                {
                    if (readData[4] == 13)
                    {
                        bedMovingStatus = BedMovingStatus.Ready;
                    }
                    if ((readData[4] == 14) || (readData[4] == 15))
                    {
                        bedMovingStatus = BedMovingStatus.PrepareToUpMoving;
                    }
                    if (readData[4] == 16)
                    {
                        bedMovingStatus = BedMovingStatus.UpMoving;
                    }
                    if (readData[4] == 17)
                    {
                        bedMovingStatus = BedMovingStatus.PrepareToDownMoving;
                    }
                    if (readData[4] == 18)
                    {
                        bedMovingStatus = BedMovingStatus.DownMoving;
                    }
                    if (readData[4] == 33)
                    {
                        bedMovingStatus = BedMovingStatus.WaitingAfterEmergency;
                    }
                }
                device.close();
                return bedMovingStatus;
            }
        }

        /// <summary>
        /// Запрос флага старт/пауза (0 - пауза, 1- старт, -1 - изначальное состояние) и флага реверса (0 - реверс не вызван, 1 - вызван, -1 - изначальное состояние)
        /// </summary>
        public void UpdateFlags()
        {
            var device = GetBedUsbDevice();
            if (device == null) { return; }

            lock (_lockObject)
            {
                var message = new byte[] { 0x6e, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff };
                device.write(message);
                var readData = device.read();
                if (readData != null)
                {
                    if (readData[5] == 0)
                    {
                        StartFlag = StartFlag.Pause;
                    }
                    if (readData[5] == 1)
                    {
                        StartFlag = StartFlag.Start;
                    }
                    if (readData[7] == 0)
                    {
                        ReverseFlag = ReverseFlag.NotReversed;
                    }
                    if (readData[7] == 1)
                    {
                        ReverseFlag = ReverseFlag.Reversed;
                    }
                }
                device.close();   
            }
        }

        /// <summary>
        /// Возвращает признак начала работы кровати
        /// </summary>
        /// <returns></returns>
        public StartFlag GetStartFlag()
        {
            var device = GetBedUsbDevice();
            if (device == null) { return StartFlag.Default; }
            lock (_lockObject)
            {
                var message = new byte[] { 0x6e, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff };
                device.write(message);
                var readData = device.read();
                if (readData != null)
                {
                    if (readData[5] == 0)
                    {
                        return StartFlag.Pause;
                    }
                    if (readData[5] == 1)
                    {
                        return StartFlag.Start;
                    }

                }
                device.close();
                return StartFlag.Default;
            }
        }

        /// <summary>
        /// Возвращает флаг реверсного движения кровати
        /// </summary>
        /// <returns></returns>
        public ReverseFlag GetReverseFlag()
        {
            var device = GetBedUsbDevice();
            if (device == null) { return ReverseFlag.Default;}

            lock (_lockObject)
            {
                var message = new byte[] { 0x6e, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff };
                device.write(message);

                var readData = device.read();
                if (readData != null)
                {
                    if (readData[7] == 0)
                    {
                        return ReverseFlag.NotReversed;
                    }
                    if (readData[7] == 1)
                    {
                        return ReverseFlag.Reversed;
                    }

                }
                device.close();
                return ReverseFlag.Default;
            }
        }

        /// <summary>
        /// Возвращает угол наклона кровати по оси Х
        /// </summary>
        /// <returns></returns>
        public Task<double> GetAngleXAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                var device = GetBedUsbDevice();
                if (device == null)
                {
                    return 0;
                }

                lock (_lockObject)
                {
                    var message = new byte[] {0x50, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};
                    device.write(message);
                    var readData = device.read();
                    int lowbyte;
                    int highbyte;
                    var replyX = 0; //первоначальное кол-во отчетов для оси Х
                    if (readData != null)
                    {
                        lowbyte = readData[3];
                        highbyte = readData[4];
                        highbyte = highbyte << 8;
                        replyX = highbyte + lowbyte;
                    }
                    message = new byte[] {0x52, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};
                    device.write(message);
                    readData = device.read();
                    var factorX = 0;
                    //множитель, на который делится кол-во отчетов, параметр прописан в прошивке и зависит от железа
                    if (readData != null)
                    {
                        lowbyte = readData[3];
                        highbyte = readData[4];
                        highbyte = highbyte << 8;
                        factorX = highbyte + lowbyte;

                    }
                    message = new byte[] {0x54, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};
                    device.write(message);
                    readData = device.read();
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
                        double mnX = factorX/1.5;
                        resultX = (replyX - initialValueX)/mnX;
                        resultX = Math.Round(resultX, 2);
                    }
                    device.close();
                    return resultX;
                }
            });
        }

        /// <summary>
        /// Возвращает угол наклона кровати по оси Y
        /// </summary>
        public double GetAngleY()
        {
            var device = GetBedUsbDevice();
            if (device == null) { return 0;}

            lock (_lockObject)
            {
                var message = new byte[] {0x51, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};
                device.write(message);
                byte[] readData = device.read();
                int lowbyteY;
                int highbyteY;
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
        }

        /// <summary>
        /// Возвращает устройство для работы с кроватью (пути к нему для дальнейшей работы с ним)
        /// </summary>
        public static HIDDevice GetBedUsbDevice()
        {
            try
            {
                var devices = HIDDevice.getConnectedDevices();
                string devicePath = null;
                foreach (var listofDevice in devices)
                {
                    if (listofDevice.product == "belmed_v1") //итоговое название всех прошивок - другое
                    {
                        devicePath = listofDevice.devicePath;
                        break;
                    }
                }
                var device = devicePath != null ? new HIDDevice(devicePath, false) : null;
                return device;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Возвращает признак подключения устройство
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            var device = GetBedUsbDevice();
            return device != null;
        }

        public void ExecuteCommand(BedControlCommand command)
        {
            try
            {
                var device = GetBedUsbDevice();
                if (device == null) { return; }
                lock (_lockObject)
                {
                    byte[] message = null;
                    switch (command)
                    {
                        case BedControlCommand.Start:
                            message = new byte[] {0x29, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};

                            break;
                        case BedControlCommand.Pause:
                            message = new byte[] {0x2a, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};

                            break;
                        case BedControlCommand.Reverse:
                            message = new byte[] {0x2c, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};
                            break;
                        case BedControlCommand.EmergencyStop:
                            message = new byte[] {0x2b, 0x00, 0x01, 0xff, 0xff, 0xff, 0xff, 0xff};
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
