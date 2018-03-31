using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.Ui.Communication;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.ViewModel.Patients
{
    public class PatientPageContext : IStoryboardPageContext
    {
        public AccessMode AccessMode { get; set; }

        public Patient Patient { get; set; }
    }
}
