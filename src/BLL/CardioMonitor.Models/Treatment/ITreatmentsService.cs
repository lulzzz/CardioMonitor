using System.Collections.Generic;

namespace CardioMonitor.BLL.CoreContracts.Treatment
{
    public interface ITreatmentsService
    {
        void Add(Treatment patient);

        List<Treatment> GetAll(int patientId);
    }
}