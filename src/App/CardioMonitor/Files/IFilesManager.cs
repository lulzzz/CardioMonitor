using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;

namespace CardioMonitor.Files
{
    public interface IFilesManager
    {
        void SaveToFile(Patient patient, Session session, string filePath = null);

        SessionContainer LoadFromFile(string filePath);
    }
}