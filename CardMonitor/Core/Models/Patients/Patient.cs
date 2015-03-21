using System;

namespace CardioMonitor.Core.Models.Patients
{
    [Serializable]
    public class Patient
    {
        public int Id { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string PatronymicName { get; set; }
    }
}
