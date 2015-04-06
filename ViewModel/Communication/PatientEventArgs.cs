using System;

namespace CardioMonitor.ViewModel.Communication
{
    public class PatientEventArgs : EventArgs
    {
        public Core.Models.Patients.Patient Patient;
        public AccessMode Mode;
    }
}
