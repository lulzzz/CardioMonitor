using System.Collections.Generic;
using CardioMonitor.Data.Common.Entities.Sessions;

namespace CardioMonitor.Data.Common.Repositories
{
    public interface ISessionsRepository
    {
        void DeleteSession(int sessionId);

        void AddSession(Session session);

        Session GetSession(int sessionId);
    }
}
