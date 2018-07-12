using System.Collections.Generic;
using System.Threading.Tasks;

namespace CardioMonitor.BLL.CoreContracts.Session
{
    public interface ISessionsService
    {
        Task<int> AddAsync(Session session);

        Task EditAsync(Session session);

        Task<Session> GetAsync(int sessionId);

        Task<ICollection<SessionWithPatientInfo>> GetAllAsync();

        Task<ICollection<SessionInfo>> GetPatientSessionInfosAsync(int patientId);

        Task DeleteAsync(int sessionId);
    }
}