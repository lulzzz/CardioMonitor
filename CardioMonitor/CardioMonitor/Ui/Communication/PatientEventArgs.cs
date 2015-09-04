using System;
using CardioMonitor.Infrastructure.Models.Patients;
using CardioMonitor.ViewModel.Communication;

namespace CardioMonitor.Ui.Communication
{
    public class PatientEventArgs : EventArgs
    {
        public Patient Patient;
        public AccessMode Mode;
    }
}
