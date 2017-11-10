using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Devices.Bed.UDP
{
    /// <summary>
    /// Тип события при обмене данными
    /// </summary>
    public enum BedMessageEventType
    {
        Read = 3, //запрос на чтение данных
        Write = 6 //запрос на запись данных
    }
}
