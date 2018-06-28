using System;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.FileSaving.Containers.V1;

namespace CardioMonitor.FileSaving.Mappers.V1
{
    internal static class SessionStatusMapperV1
    {
        public static SessionStatus ToDomain(this StoredSessionStatusV1 status)
        {
            switch (status)
            {
                case StoredSessionStatusV1.NotStarted:
                    return SessionStatus.NotStarted;
                case StoredSessionStatusV1.Completed:
                    return SessionStatus.Completed;
                case StoredSessionStatusV1.TerminatedOnError:
                    return SessionStatus.TerminatedOnError;
                case StoredSessionStatusV1.InProgress:
                    return SessionStatus.InProgress;
                case StoredSessionStatusV1.Suspended:
                    return SessionStatus.Suspended;
                case StoredSessionStatusV1.EmergencyStopped:
                    return SessionStatus.EmergencyStopped;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public static StoredSessionStatusV1 ToStored(this SessionStatus status)
        {
            switch (status)
            {
                case SessionStatus.NotStarted:
                    return StoredSessionStatusV1.NotStarted;
                case SessionStatus.Completed:
                    return StoredSessionStatusV1.Completed;
                case SessionStatus.TerminatedOnError:
                    return StoredSessionStatusV1.TerminatedOnError;
                case SessionStatus.InProgress:
                    return StoredSessionStatusV1.InProgress;
                case SessionStatus.Suspended:
                    return StoredSessionStatusV1.Suspended;
                case SessionStatus.EmergencyStopped:
                    return StoredSessionStatusV1.EmergencyStopped;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }
}
