using System.Collections.Generic;
using System.Threading.Tasks;

namespace CardioMonitor.BLL.CoreContracts.Session
{
    public interface ISessionsService
    {
        Task AddAsync(Session session);

        Task<Session> GetAsync(int sessionId);

        Task<ICollection<SessionWithPatientInfo>> GetAllAsync();

        Task<ICollection<SessionInfo>> GetPatientSessionInfosAsync(int patientId);

        Task DeleteAsync(int sessionId);
    }
}