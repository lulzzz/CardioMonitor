using System.Threading.Tasks;

namespace CardioMonitor.Repositories.Abstract
{
    public interface IDataBaseRepository
    {
        bool GetConnectionStatus();

        Task CheckConnectionAsync(string dataBase, string source, string user, string password);
    }
}