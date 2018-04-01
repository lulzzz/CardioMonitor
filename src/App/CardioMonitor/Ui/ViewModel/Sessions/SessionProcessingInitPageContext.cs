using CardioMonitor.BLL.CoreContracts.Patients;
using JetBrains.Annotations;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    public class SessionProcessingInitPageContext : IStoryboardPageContext
    {
        [CanBeNull]
        public Patient Patient { get; set; }
    }
}