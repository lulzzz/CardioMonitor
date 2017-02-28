using CardioMonitor.BLL.CoreContracts.Treatment;
using CardioMonitor.Data.Contracts.Entities.Treatments;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.Mappers
{
    public static class TreatmentsMapper
    {
        public static TreatmentEntity ToEntity([NotNull] this Treatment domain)
        {
            return new TreatmentEntity
            {
                Id = domain.Id,
                PatientId = domain.PatientId,
                StartDate = domain.StartDate
            };
        }

        public static Treatment ToDomain([NotNull] this TreatmentEntity entity)
        {
            return new Treatment
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                StartDate = entity.StartDate
            };
        }
    }
}