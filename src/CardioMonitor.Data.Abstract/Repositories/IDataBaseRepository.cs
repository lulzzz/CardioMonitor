using System.Threading.Tasks;

namespace CardioMonitor.Data.Common.Repositories
{
    public interface IDataBaseRepository
    {
        bool GetConnectionStatus();

        Task CheckConnectionAsync(string dataBase, string source, string user, string password);
    }
}