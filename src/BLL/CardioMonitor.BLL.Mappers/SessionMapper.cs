using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Data.Contracts.Entities.Sessions;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.Mappers
{
    public static class SessionMapper
    {
        [NotNull]
        public static SessionEntity ToEntity([NotNull] this Session session)
        {
            return new SessionEntity
            {
                DateTime = session.DateTime,
                Id = session.Id,
                Status = session.Status.ToSessionCompletionStatus(),
                TreatmentId = session.TreatmentId

            };
        }


        [NotNull]
        public static Session ToDomain([NotNull] this SessionEntity session)
        {
            return new Session
            {
                DateTime = session.DateTime,
                Id = session.Id,
                Status = session.Status.ToSessionStatus(),
                TreatmentId = session.TreatmentId
            };
        }
    }
}