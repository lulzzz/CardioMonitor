using System;
using CardioMonitor.Core.Models.Patients;

namespace CardioMonitor.Core.Models.Session
{
    [Serializable]
    public class SessionContainer
    {
        public Patient Patient { get; set; }

        public Core.Models.Session.Session Session { get; set; }
    }
}
