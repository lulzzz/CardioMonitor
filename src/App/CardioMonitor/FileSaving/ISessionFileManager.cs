using System.Threading.Tasks;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using JetBrains.Annotations;

namespace CardioMonitor.FileSaving
{
    public interface ISessionFileManager
    {
        void Save([NotNull] Patient patient, [NotNull] Session session, string filePath = null);

        [NotNull]
        SessionContainer Load([NotNull] string filePath);
    }
}