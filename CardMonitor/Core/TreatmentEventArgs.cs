using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Core
{
    public class TreatmentEventArgs : EventArgs
    {
        public TreatmentAction Action { get; set; }
    }
}
