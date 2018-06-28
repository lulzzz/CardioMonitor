using System;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.FileSaving.Containers.V1;
using JetBrains.Annotations;

namespace CardioMonitor.FileSaving.Mappers.V1
{
    internal static class PatientMapperV1
    {
        public static Patient ToDomain([NotNull] this StoredPatientV1 patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));
            
            return new Patient
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                PatronymicName = patient.PatronymicName,
                LastName = patient.LastName,
                BirthDate = patient.BirthDate
            };
        }
        
        public static StoredPatientV1 ToStored([NotNull] this Patient patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));
            
            return new StoredPatientV1 
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                PatronymicName = patient.PatronymicName,
                LastName = patient.LastName,
                BirthDate = patient.BirthDate
            };
        }
    }
}