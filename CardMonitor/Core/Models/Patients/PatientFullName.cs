using System;

namespace CardioMonitor.Core.Models.Patients
{
    [Serializable]
    public class PatientFullName
    {
        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string PatronymicName { get; set; }

        public string Name
        {
            get
            {
                return String.Format("{0} {1} {2}", LastName, FirstName, PatronymicName);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
