using System.Collections.Generic;

namespace CardioMonitor.BLL.CoreContracts.Session
{
    public interface ISessionsService
    {
        void Add(Session session);

        Session Get(int sessionId);

        List<Session> GetAll(int treatmentId);


        List<SessionInfo> GetInfos(int treatmentId);

        void Delete(int sessionId);
    }
}