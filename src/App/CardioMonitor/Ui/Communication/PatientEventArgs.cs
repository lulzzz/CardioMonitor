using System;
using CardioMonitor.BLL.CoreContracts.Patients;

namespace CardioMonitor.Ui.Communication
{
    public class PatientEventArgs : EventArgs
    {
        public Patient Patient;
        public AccessMode Mode;
    }
}
