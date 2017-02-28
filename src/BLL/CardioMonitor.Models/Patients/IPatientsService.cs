using System.Collections.Generic;

namespace CardioMonitor.BLL.CoreContracts.Patients
{
    public interface IPatientsService
    {
        void Add(Patient patient);

        List<Patient> GetAll();

        void Edit(Patient patient);

        void Delete(int patientId);
    }
}