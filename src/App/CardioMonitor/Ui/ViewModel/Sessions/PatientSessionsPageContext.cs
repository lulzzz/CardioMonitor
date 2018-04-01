using CardioMonitor.BLL.CoreContracts.Patients;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    internal class PatientSessionsPageContext : IStoryboardPageContext
    {
        /// <summary>
        /// Пациент, сеансы которого необходимо отобразить
        /// </summary>
        public Patient Patient { get; set; }
    }
}
