using System.Collections.Generic;
using CardioMonitor.Models.Patients;

namespace CardioMonitor.Repositories.Abstract
{
    public interface IPatientsRepository
    {
        List<Patient> GetPatients();

        void AddPatient(Patient patient);

        void UpdatePatient(Patient patient);

        void DeletePatient(int patientId);
    }
}