using System.Collections.Generic;

namespace CardioMonitor.BLL.CoreContracts.Patients
{
    public interface IPatientsService
    {
        void Add(Patient patient);

        List<PatientFullName> GetPatientNames(ICollection<int> patientIds);


        List<Patient> GetAll();

        void Edit(Patient patient);

        void Delete(int patientId);
    }
}