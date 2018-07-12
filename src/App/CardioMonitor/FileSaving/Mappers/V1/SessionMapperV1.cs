using System;
using System.Linq;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.FileSaving.Containers.V1;
using JetBrains.Annotations;

namespace CardioMonitor.FileSaving.Mappers.V1
{
    internal static class SessionMapperV1
    {
        public static Session ToDomain([NotNull] this StoredSessionV1 session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            return new Session
            {
                Id = session.Id,
                PatientId = session.PatientId,
                Status = session.Status.ToDomain(),
                TimestampUtc = session.TimestampUtc,
                Cycles = session.Cycles
                    .Select(x => x.ToDomain())
                    .ToList()
            };
        }
        
        public static StoredSessionV1 ToStored([NotNull] this Session session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            return new StoredSessionV1
            {
                Id = session.Id,
                PatientId = session.PatientId,
                Status = session.Status.ToStored(),
                TimestampUtc = session.TimestampUtc,
                Cycles = session.Cycles
                    .Select(x => x.ToStored())
                    .ToList()
            };
        }
    }
}