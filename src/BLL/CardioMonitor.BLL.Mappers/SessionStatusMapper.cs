using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Data.Contracts.Entities.Sessions;

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
                case SessionCompletionStatus.Terminated:
                    return SessionStatus.TerminatedOnError;
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
                    return SessionCompletionStatus.Terminated;
                default:
                    return SessionCompletionStatus.Unknown;
            }
        }
    }
}