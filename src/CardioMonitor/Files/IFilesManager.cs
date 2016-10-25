using CardioMonitor.Models.Patients;
using CardioMonitor.Models.Session;

namespace CardioMonitor.Files
{
    public interface IFilesManager
    {
        void SaveToFile(Patient patient, Session session, string filePath = null);

        SessionContainer LoadFromFile(string filePath);
    }
}