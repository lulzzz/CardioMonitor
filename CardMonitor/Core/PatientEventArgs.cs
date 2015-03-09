using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardioMonitor.Patients;

namespace CardioMonitor.Core
{
    public class PatientEventArgs : EventArgs
    {
        public Patient Patient;
        public AccessMode Mode;
    }
}
