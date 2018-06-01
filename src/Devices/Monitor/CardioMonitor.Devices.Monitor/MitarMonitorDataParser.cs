using System;
using CardioMonitor.Devices.Monitor.Infrastructure;

namespace CardioMonitor.Devices.Monitor
{
    public class MitarMonitorDataParser
    {

        public PatientCommonParams GetPatientCommonParams(byte[] message)
        {
            int iterator = 0;
            
            if (message[iterator] >> 4 != 0xE0) throw new ArgumentException("неверный формат пакета");
            
            //todo бла бла получили числа - дабавили в параметр
            
            
            
           return new PatientCommonParams(); 
        }

        public PatientPressureParams GetPatientPressureParams(byte[] message)
        {
            
        }
        
        
    }
}