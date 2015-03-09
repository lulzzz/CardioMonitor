using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardioMonitor.Patients.Treatments
{
    public class Treatment
    {
        public int Id { get; set; }

        public int PatientId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime LastSessionDate { get; set; }

        public int SessionsCount { get; set; }

        public Treatment()
        {
            StartDate = new DateTime();
            LastSessionDate = new DateTime();
        }
    }
}
