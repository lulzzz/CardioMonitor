using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Patients.Session
{
    [Serializable]
    public class Session
    {
        public int Id { get; set; }

        public int TreatmentId { get; set; }

        public DateTime DateTime { get; set; }

        public SessionStatus Status { get; set; }

        public ObservableCollection<PatientParams> PatientParams { get; set; }

        public Session()
        {
            DateTime = new DateTime();
            Status = SessionStatus.Unknown;
            PatientParams = new ObservableCollection<PatientParams>();
        }
    }
}
