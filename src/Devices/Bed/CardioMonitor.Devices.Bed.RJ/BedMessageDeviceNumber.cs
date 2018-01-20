using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Devices.Bed.UDP
{
    //TODO название класса не отражает суть содержимого
   public enum BedMessageDeviceNumber
    {
       CentralBoard = 1,
       XEngineBoard,
       YEngineBoard,
       ControlPanelBoard
    }
}
