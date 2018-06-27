using CardioMonitor.BLL.CoreContracts.Patients;

namespace CardioMonitor.BLL.CoreContracts.Session
{
    public class SessionContainer
    {
        public Patient Patient { get; set; }

        public Session Session { get; set; }
    }
}
