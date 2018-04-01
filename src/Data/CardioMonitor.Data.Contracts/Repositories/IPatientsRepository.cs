using System.Collections.Generic;
using CardioMonitor.Data.Contracts.Entities.Patients;

namespace CardioMonitor.Data.Contracts.Repositories
{
    public interface IPatientsRepository
    {
        List<PatientEntity> GetPatients();

        List<PatientEntity> GetPatients(ICollection<int> patientIds);

        void AddPatient(PatientEntity patientEntity);

        void UpdatePatient(PatientEntity patientEntity);

        void DeletePatient(int patientId);
    }
}