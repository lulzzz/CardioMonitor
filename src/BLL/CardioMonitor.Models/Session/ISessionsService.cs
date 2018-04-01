using System.Collections.Generic;

namespace CardioMonitor.BLL.CoreContracts.Session
{
    public interface ISessionsService
    {
        void Add(Session session);

        Session Get(int sessionId);

        List<Session> GetAll(int patientId);


        List<SessionInfo> GetInfos(int patientId);

        void Delete(int sessionId);
    }
}