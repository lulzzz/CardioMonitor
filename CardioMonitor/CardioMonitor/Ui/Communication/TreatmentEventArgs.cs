using System;
using CardioMonitor.Core;

namespace CardioMonitor.ViewModel.Communication
{
    public class TreatmentEventArgs : EventArgs
    {
        public TreatmentAction Action { get; set; }
    }
}
