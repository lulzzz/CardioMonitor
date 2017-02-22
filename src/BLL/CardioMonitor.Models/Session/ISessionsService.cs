using System.Collections.Generic;
using CardioMonitor.BLL.CoreContracts.Patients;

namespace CardioMonitor.BLL.CoreContracts.Session
{
    public interface ISessionsService
    {
        void Add(Session patient);

        List<Session> GetAll();

        void Edit(Session patient);

        void Delete(Session patient);
    }
}