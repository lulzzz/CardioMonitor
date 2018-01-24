using System;

namespace CardioMonitor.Devices
{
    /// <summary>
    /// Ошибка подключения к устройству
    /// </summary>
    public class DeviceConnectionException : Exception
    { 

        public DeviceConnectionException()
        {
        }

        public DeviceConnectionException(string message) : base(message)
        {
        }

        public DeviceConnectionException(string message, Exception exception) : base(message, exception)
        {
        }
        
    }
}