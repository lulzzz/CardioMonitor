using System;

namespace CardioMonitor.Ui.Communication
{
    public class TreatmentEventArgs : EventArgs
    {
        public TreatmentAction Action { get; set; }
    }
}
