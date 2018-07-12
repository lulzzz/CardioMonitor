using System.Threading.Tasks;

namespace CardioMonitor.Data.Contracts.Repositories
{
    public interface IDataBaseRepository
    {
        bool GetConnectionStatus();

        Task CheckConnectionAsync(string dataBase, string source, string user, string password);
    }
}