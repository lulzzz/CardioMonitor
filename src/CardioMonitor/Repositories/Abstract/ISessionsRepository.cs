using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardioMonitor.Models.Session;

namespace CardioMonitor.Repositories.Abstract
{
    public interface ISessionsRepository
    {
        List<SessionInfo> GetSessionInfos(int treatmentId);
        void DeleteSession(int sessionId);
        void AddSession(Session session);

        Session GetSession(int sessionId);
    }
}
