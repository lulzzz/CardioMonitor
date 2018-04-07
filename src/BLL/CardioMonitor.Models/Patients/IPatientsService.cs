using System.Collections.Generic;
using System.Threading.Tasks;

namespace CardioMonitor.BLL.CoreContracts.Patients
{
    public interface IPatientsService
    {
        Task AddAsync(Patient patient);

        Task<ICollection<PatientFullName>> GetPatientNamesAsync(ICollection<int> patientIds);

        Task<ICollection<PatientFullName>> GetPatientNamesAsync();

        Task<Patient> GetPatientAsync(int patientId);

        Task<ICollection<Patient>> GetAllAsync();

        Task EditAsync(Patient patient);

        Task DeleteAsync(int patientId);
    }
}