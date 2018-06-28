using System;
using System.Linq;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.FileSaving.Containers.V1;
using JetBrains.Annotations;

namespace CardioMonitor.FileSaving.Mappers.V1
{
    internal static class SessionCycleMapperV1
    {
        public static SessionCycle ToDomain([NotNull] this StoredSessionCycleV1 sessionCycle)
        {
            if (sessionCycle == null) throw new ArgumentNullException(nameof(sessionCycle));

            return new SessionCycle
            {
                Id = sessionCycle.Id,
                CycleNumber = sessionCycle.CycleNumber,
                SessionId = sessionCycle.SessionId,
                PatientParams = sessionCycle.PatientParams
                    .Select(x => x.ToDomain())
                    .ToList()
            };
        }
        
        public static StoredSessionCycleV1 ToStored([NotNull] this SessionCycle  sessionCycle)
        {
            if (sessionCycle == null) throw new ArgumentNullException(nameof(sessionCycle));

            return new StoredSessionCycleV1
            {
                Id = sessionCycle.Id,
                CycleNumber = sessionCycle.CycleNumber,
                SessionId = sessionCycle.SessionId,
                PatientParams = sessionCycle.PatientParams
                    .Select(x => x.ToStored())
                    .ToList()
            };
        }
    }
}