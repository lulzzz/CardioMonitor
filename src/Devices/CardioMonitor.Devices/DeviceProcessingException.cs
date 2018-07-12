using System;

namespace CardioMonitor.Devices
{
    /// <summary>
    /// Ошибка в ходе работы с устройством
    /// </summary>
    public class DeviceProcessingException : Exception
    { 

        public DeviceProcessingException()
        {
        }

        public DeviceProcessingException(string message) : base(message)
        {
        }

        public DeviceProcessingException(string message, Exception exception) : base(message, exception)
        {
        }
        
    }
}