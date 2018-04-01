using System.Collections.Generic;
using CardioMonitor.Data.Contracts.Entities.Sessions;

namespace CardioMonitor.Data.Contracts.Repositories
{
    public interface ISessionsRepository
    {
        void DeleteSession(int sessionId);

        void AddSession(SessionEntity sessionEntity);

        SessionEntity GetSession(int sessionId);

        List<SessionEntity> GetSessions(int patientId);


        List<SessionEntity> GetSessions();
    }
}
