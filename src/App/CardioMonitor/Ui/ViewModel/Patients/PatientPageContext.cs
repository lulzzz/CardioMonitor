using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.Infrastructure.WpfCommon.Communication;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.ViewModel.Patients
{
    public class PatientPageContext : IStoryboardPageContext
    {
        public AccessMode Mode { get; set; }

        public Patient Patient { get; set; }
    }
}
