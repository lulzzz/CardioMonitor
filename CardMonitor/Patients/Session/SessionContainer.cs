using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Patients.Session
{
    [Serializable]
    public class SessionContainer
    {
        public Patient Patient { get; set; }

        public Session Session { get; set; }
    }
}
