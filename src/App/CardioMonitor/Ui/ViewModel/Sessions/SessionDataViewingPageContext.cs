using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    public class SessionDataViewingPageContext : IStoryboardPageContext
    {
        public Patient Patient { get; set; }

        public Session Session { get; set; }
    }
}