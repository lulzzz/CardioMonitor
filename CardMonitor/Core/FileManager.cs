using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CardioMonitor.Core.Models.Patients;
using CardioMonitor.Core.Models.Session;

namespace CardioMonitor.Core
{
    public static class FileManager
    {
        public static void SaveToFile(Patient patient, Session session)
        {
            if (patient == null) throw new ArgumentNullException("patient");
            if (session == null) throw new ArgumentNullException("session");

            var dirPath = String.Format("{0}_{1}_{2}_{3}", patient.LastName, patient.FirstName, patient.PatronymicName,
                patient.Id);
            dirPath = Path.Combine(Settings.Settings.Instance.FilesDirectoryPath, dirPath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var dateSring = String.Format("{0}_{1}_{2}_{3}_{4}_{5}", session.DateTime.Day,
                                                                     session.DateTime.Month,
                                                                     session.DateTime.Year,
                                                                     session.DateTime.Hour,
                                                                     session.DateTime.Minute, 
                                                                     session.DateTime.Second);
            var fileName = String.Format("{0}_{1}_{2}_{3}_{4}.cmsf", patient.LastName, 
                                                                     patient.FirstName,
                                                                     patient.PatronymicName, 
                                                                     patient.Id, 
                                                                     dateSring);

            var container = new SessionContainer
            {
                Patient = patient,
                Session = session
            };

            using (var savingStream = new FileStream(Path.Combine(dirPath, fileName), FileMode.Create))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(savingStream, container);
                savingStream.Close();
            }
        }

        public static SessionContainer LoadFromFile(string filePath)
        {
            SessionContainer container;
            using (var loadingStream = new FileStream(filePath, FileMode.Open))
            {
                var bf = new BinaryFormatter();
                container = (SessionContainer) bf.Deserialize(loadingStream);
                loadingStream.Close();
            }
            return container;
        }
    }
}
