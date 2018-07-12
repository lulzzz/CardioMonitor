using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.Data.Ef.Entities.Patients;

namespace CardioMonitor.BLL.Mappers
{
    public static class PatientMapper
    {
        public static PatientEntity ToEntity(this Patient model)
        {
            return new PatientEntity
            {
                Id =  model.Id,
                BirthDate = model.BirthDate,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PatronymicName = model.PatronymicName
            };
        }

        public static Patient ToDomain(this PatientEntity entity)
        {
            return new Patient
            {
                Id = entity.Id,
                BirthDate = entity.BirthDate,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                PatronymicName = entity.PatronymicName
            };
        }
    }
}