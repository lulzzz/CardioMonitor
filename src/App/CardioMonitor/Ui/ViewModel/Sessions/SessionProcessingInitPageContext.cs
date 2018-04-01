using Markeli.Storyboards;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    public class SessionProcessingInitPageContext : IStoryboardPageContext
    {
        public int? PatientId { get; set; }
    }
}