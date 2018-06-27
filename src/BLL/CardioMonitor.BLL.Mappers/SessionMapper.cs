using System;
using System.Linq;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Data.Ef.Entities.Sessions;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.Mappers
{
    public static class DeviceValueStatusMapper
    {
        public static DeviceValueStatus ToDomain(this DaoDeviceValueStatus status)
        {
            switch (status)
            {
                case DaoDeviceValueStatus.Unknown:
                    return DeviceValueStatus.Unknown;
                case DaoDeviceValueStatus.NotObtained:
                    return DeviceValueStatus.NotObtained;
                case DaoDeviceValueStatus.Obtained:
                    return DeviceValueStatus.Obtained;
                case DaoDeviceValueStatus.ErrorOccured:
                    return DeviceValueStatus.ErrorOccured;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
        
        public static DaoDeviceValueStatus ToEntity(this DeviceValueStatus status)
        {
            switch (status)
            {
                case DeviceValueStatus.Unknown:
                    return DaoDeviceValueStatus.Unknown;
                case DeviceValueStatus.NotObtained:
                    return DaoDeviceValueStatus.NotObtained;
                case DeviceValueStatus.Obtained:
                    return DaoDeviceValueStatus.Obtained;
                case DeviceValueStatus.ErrorOccured:
                    return DaoDeviceValueStatus.ErrorOccured;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }
    
    //todo уточнить
    public static class SessionMapper
    {
        [NotNull]
        public static SessionEntity ToEntity([NotNull] this Session session)
        {
            var entity =  new SessionEntity
            {
                TimestampUtc = session.TimestampUtc,
                Id = session.Id,
                Status = session.Status.ToSessionCompletionStatus(),
                PatientId = session.PatientId,
                Cycles = session.Cycles.Select(x =>
                {
                    var cycle = x.ToEntity();
                    cycle.SessionId = session.Id;
                    return cycle;
                }).ToList()
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
                TimestampUtc = session.TimestampUtc,
                Id = session.Id,
                Status = session.Status.ToSessionStatus(),
                PatientId = session.PatientId,
                Cycles = session.Cycles.Select(x => x.ToDomain()).ToList()
            };
        }

        [NotNull]
        public static SessionInfo ToInfoDomain([NotNull] this SessionEntity session)
        {
            return new SessionInfo
            {
                TimestampUtc = session.TimestampUtc,
                Id = session.Id,
                Status = session.Status.ToSessionStatus()
            };
        }

        public static SessionCycle ToDomain([NotNull] this SessionCycleEntity entity)
        {
            return new SessionCycle
            {
                Id = entity.Id,
                CycleNumber = entity.CycleNumber,
                SessionId = entity.SessionId,
                PatientParams = entity.PatientParams.Select(x => x.ToDomain()).ToList()
            };
        }

        public static SessionCycleEntity ToEntity([NotNull] this SessionCycle domain)
        {
            var entity = new SessionCycleEntity
            {
                Id = domain.Id,
                CycleNumber = domain.CycleNumber,
                SessionId = domain.SessionId,
                PatientParams = domain.PatientParams.Select(x =>
                {
                    var patientParams = x.ToEntity();
                    patientParams.SessionCycleId = domain.Id;
                    return patientParams;
                }).ToList()
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
                AverageArterialPressure = new DeviceValue<short>(
                    entity.AverageArterialPressure,
                    entity.AverageArterialPressureStatus.ToDomain()),
                DiastolicArterialPressure = new DeviceValue<short>(
                    entity.DiastolicArterialPressure,
                    entity.DiastolicArterialPressureStatus.ToDomain()),
                HeartRate = new DeviceValue<short>(
                    entity.HeartRate,
                    entity.HeartRateStatus.ToDomain()),
                Id = entity.Id,
                InclinationAngle = entity.InclinationAngle,
                Iteraton = entity.Iteration,
                RepsirationRate = new DeviceValue<short>(
                    entity.RepsirationRate,
                    entity.RepsirationRateStatus.ToDomain()),
                SessionCycleId = entity.SessionCycleId,
                Spo2 = new DeviceValue<short>(
                    entity.Spo2,
                    entity.Spo2Status.ToDomain()),
                SystolicArterialPressure = new DeviceValue<short>(
                    entity.SystolicArterialPressure,
                    entity.SystolicArterialPressureStatus.ToDomain())
            };
        }

        public static PatientParamsEntity ToEntity([NotNull] this PatientParams domain)
        {
            return new PatientParamsEntity
            {
                AverageArterialPressure = domain.AverageArterialPressure.Value,
                AverageArterialPressureStatus = domain.AverageArterialPressure.Status.ToEntity(),
                DiastolicArterialPressure = domain.DiastolicArterialPressure.Value,
                DiastolicArterialPressureStatus = domain.DiastolicArterialPressure.Status.ToEntity(),
                HeartRate = domain.HeartRate.Value,
                HeartRateStatus = domain.HeartRate.Status.ToEntity(),
                Id = domain.Id,
                SessionCycleId = domain.SessionCycleId,
                InclinationAngle = domain.InclinationAngle,
                Iteration = domain.Iteraton,
                RepsirationRate = domain.RepsirationRate.Value,
                RepsirationRateStatus = domain.RepsirationRate.Status.ToEntity(),
                Spo2 = domain.Spo2.Value,
                Spo2Status = domain.Spo2.Status.ToEntity(),
                SystolicArterialPressure = domain.SystolicArterialPressure.Value,
                SystolicArterialPressureStatus = domain.SystolicArterialPressure.Status.ToEntity()
            };
        }
    }
}