using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Data.Ef.Entities.Sessions;

namespace CardioMonitor.BLL.Mappers
{
    public static class SessionStatusMapper
    {
        public static SessionStatus ToSessionStatus(this SessionCompletionStatus status)
        {
            switch (status)
            {
                case SessionCompletionStatus.Completed:
                    return SessionStatus.Completed;
                case SessionCompletionStatus.TerminatedOnError:
                    return SessionStatus.TerminatedOnError;
                case SessionCompletionStatus.NotStarted:
                    return SessionStatus.NotStarted;
                case SessionCompletionStatus.InProgress:
                    return SessionStatus.InProgress;
                case SessionCompletionStatus.Suspended:
                    return SessionStatus.Suspended;
                case SessionCompletionStatus.EmergencyStopped:
                    return SessionStatus.EmergencyStopped;
                default:
                    return SessionStatus.NotStarted;
            }
        }


        public static SessionCompletionStatus ToSessionCompletionStatus(this SessionStatus status)
        {
            switch (status)
            {
                case SessionStatus.Completed:
                    return SessionCompletionStatus.Completed;
                case SessionStatus.TerminatedOnError:
                    return SessionCompletionStatus.TerminatedOnError;
                case SessionStatus.NotStarted:
                    return SessionCompletionStatus.NotStarted;
                case SessionStatus.InProgress:
                    return SessionCompletionStatus.InProgress;
                case SessionStatus.Suspended:
                    return SessionCompletionStatus.Suspended;
                case SessionStatus.EmergencyStopped:
                    return SessionCompletionStatus.EmergencyStopped;
                default:
                    return SessionCompletionStatus.NotStarted;
            }
        }
    }
}