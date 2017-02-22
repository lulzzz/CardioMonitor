using System.Collections.Generic;
using CardioMonitor.Data.Contracts.Entities.Treatments;

namespace CardioMonitor.Data.Contracts.Repositories
{
    public interface ITreatmentsRepository
    {
        List<TreatmentEntity> GetTreatments(int patientId);

        void AddTreatment(TreatmentEntity treatmentEntity);

        void DeleteTreatment(int treatmentId);
    }
}
