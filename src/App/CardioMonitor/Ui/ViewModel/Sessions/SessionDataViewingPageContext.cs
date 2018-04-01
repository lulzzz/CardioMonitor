using CardioMonitor.BLL.CoreContracts.Patients;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    public class SessionDataViewingPageContext : IStoryboardPageContext
    {
        public int PatientId { get; set; }

        public int SessionId { get; set; }

        /// <summary>
        /// Полный путь к сеансу, сохраненному на диске
        /// </summary>
        public string FileFullPath { get; set; }
    }
}