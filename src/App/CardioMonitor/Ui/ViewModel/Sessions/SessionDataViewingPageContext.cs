using CardioMonitor.BLL.CoreContracts.Patients;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    public class SessionDataViewingPageContext : IStoryboardPageContext
    {
        public Patient Patient { get; set; }

        public int SessionId { get; set; }
    }
}