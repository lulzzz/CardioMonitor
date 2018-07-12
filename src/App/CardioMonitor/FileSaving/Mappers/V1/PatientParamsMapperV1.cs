using System;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.FileSaving.Containers.V1;
using JetBrains.Annotations;

namespace CardioMonitor.FileSaving.Mappers.V1
{
    internal static class PatientParamsMapperV1
    {
        public static PatientParams ToDomain([NotNull] this StoredPatientParamsV1 patientParams)
        {
            if (patientParams == null) throw new ArgumentNullException(nameof(patientParams));
            
            return new PatientParams
            {
                Id = patientParams.Id,
                InclinationAngle = patientParams.InclinationAngle,
                Iteraton =  patientParams.Iteraton,
                SessionCycleId = patientParams.SessionCycleId,
                AverageArterialPressure = patientParams.AverageArterialPressure.ToDomain(),
                DiastolicArterialPressure = patientParams.DiastolicArterialPressure.ToDomain(),
                HeartRate = patientParams.HeartRate.ToDomain(),
                RepsirationRate = patientParams.RepsirationRate.ToDomain(),
                Spo2 = patientParams.Spo2.ToDomain(),
                SystolicArterialPressure = patientParams.SystolicArterialPressure.ToDomain()
            };
        }
        
        public static StoredPatientParamsV1 ToStored([NotNull] this PatientParams  patientParams)
        {
            if (patientParams == null) throw new ArgumentNullException(nameof(patientParams));
            
            return new StoredPatientParamsV1 
            {
                Id = patientParams.Id,
                InclinationAngle = patientParams.InclinationAngle,
                Iteraton =  patientParams.Iteraton,
                SessionCycleId = patientParams.SessionCycleId,
                AverageArterialPressure = patientParams.AverageArterialPressure.ToStored(),
                DiastolicArterialPressure = patientParams.DiastolicArterialPressure.ToStored(),
                HeartRate = patientParams.HeartRate.ToStored(),
                RepsirationRate = patientParams.RepsirationRate.ToStored(),
                Spo2 = patientParams.Spo2.ToStored(),
                SystolicArterialPressure = patientParams.SystolicArterialPressure.ToStored()
            };
        }
    }
}