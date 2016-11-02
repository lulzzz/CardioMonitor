using System.Collections.Generic;
using CardioMonitor.Data.Common.Entities.Patients;

namespace CardioMonitor.Data.Common.Repositories
{
    public interface IPatientsRepository
    {
        List<Patient> GetPatients();

        void AddPatient(Patient patient);

        void UpdatePatient(Patient patient);

        void DeletePatient(int patientId);
    }
}