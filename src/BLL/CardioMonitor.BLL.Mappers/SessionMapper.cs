using System.Linq;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Data.Contracts.Entities.Sessions;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.Mappers
{
    //todo уточнить
    public static class SessionMapper
    {
        [NotNull]
        public static SessionEntity ToEntity([NotNull] this Session session)
        {
            var entity =  new SessionEntity
            {
                DateTime = session.DateTime,
                Id = session.Id,
                Status = session.Status.ToSessionCompletionStatus(),
                TreatmentId = session.PatientId,
                Cycles = session.Cycles.Select(x => x.ToEntity()).ToList()
            };
            foreach (var sessionCycleEntity in entity.Cycles)
            {
                sessionCycleEntity.SessionEntity = entity;
            }

            return entity;
        }


        [NotNull]
        public static Session ToDomain([NotNull] this SessionEntity session)
        {
            return new Session
            {
                DateTime = session.DateTime,
                Id = session.Id,
                Status = session.Status.ToSessionStatus(),
                PatientId = session.TreatmentId
            };
        }

        [NotNull]
        public static SessionInfo ToInfoDomain([NotNull] this SessionEntity session)
        {
            return new SessionInfo
            {
                DateTime = session.DateTime,
                Id = session.Id,
                Status = session.Status.ToSessionStatus()
            };
        }

        public static SessionCycle ToDomain([NotNull] this SessionCycleEntity entity)
        {
            return new SessionCycle
            {
                CycleNumber = entity.CycleNumber,
                SessionId = entity.SessionId,
                PatientParams = entity.PatientParams.Select(x => x.ToDomain()).ToList()
            };
        }

        public static SessionCycleEntity ToEntity([NotNull] this SessionCycle domain)
        {
            var entity = new SessionCycleEntity
            {
                CycleNumber = domain.CycleNumber,
                SessionId = domain.SessionId,
                PatientParams = domain.PatientParams.Select(x => x.ToEntity()).ToList()
            };
            foreach (var patientParamsEntity in entity.PatientParams)
            {
                patientParamsEntity.SessionCycleEntity = entity;
            }
            return entity;
        }

        public static PatientParams ToDomain([NotNull] this PatientParamsEntity entity)
        {
            return new PatientParams
            {
                AverageArterialPressure = entity.AverageArterialPressure,
                DiastolicArterialPressure = entity.DiastolicArterialPressure,
                HeartRate = entity.HeartRate,
                Id = entity.Id,
                InclinationAngle = entity.InclinationAngle,
                Iteraton = entity.Iteration,
                RepsirationRate = entity.RepsirationRate,
                Spo2 = entity.Spo2,
                SystolicArterialPressure = entity.SystolicArterialPressure
            };
        }

        public static PatientParamsEntity ToEntity([NotNull] this PatientParams domain)
        {
            return new PatientParamsEntity
            {
                AverageArterialPressure = domain.AverageArterialPressure,
                DiastolicArterialPressure = domain.DiastolicArterialPressure,
                HeartRate = domain.HeartRate,
                InclinationAngle = domain.InclinationAngle,
                Iteration = domain.Iteraton,
                RepsirationRate = domain.RepsirationRate,
                Spo2 = domain.Spo2,
                SystolicArterialPressure = domain.SystolicArterialPressure
            };
        }
    }
}