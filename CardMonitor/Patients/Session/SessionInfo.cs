using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Patients.Session
{
    /// <summary>
    /// Краткая информация о сеансе
    /// </summary>
    public class SessionInfo
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public SessionStatus Status { get; set; }
    }
}
