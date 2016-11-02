using System.Collections.Generic;
using CardioMonitor.Data.Common.Entities.Treatments;

namespace CardioMonitor.Data.Common.Repositories
{
    public interface ITreatmentsRepository
    {
        List<Treatment> GetTreatments(int patientId);

        void AddTreatment(Treatment treatment);

        void DeleteTreatment(int treatmentId);
    }
}
