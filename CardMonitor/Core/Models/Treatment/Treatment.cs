using System;

namespace CardioMonitor.Core.Models.Treatment
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
