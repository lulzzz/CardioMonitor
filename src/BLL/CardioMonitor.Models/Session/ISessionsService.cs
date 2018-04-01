using System.Collections.Generic;

namespace CardioMonitor.BLL.CoreContracts.Session
{
    public interface ISessionsService
    {
        void Add(Session session);

        Session Get(int sessionId);

        List<SessionWithPatientInfo> GetSessions();
        
        List<SessionInfo> GetPatientSessionInfos(int patientId);

        void Delete(int sessionId);
    }
}