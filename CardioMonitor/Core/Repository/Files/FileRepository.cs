using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CardioMonitor.Core.Models.Patients;
using CardioMonitor.Core.Models.Session;
using CardioMonitor.Logs;
using CardioMonitor.Resources;

namespace CardioMonitor.Core.Repository.Files
{
    /// <summary>
    /// Репозиторий для доступа к файлам
    /// </summary>
    public static class FileRepository
    {
        /// <summary>
        /// Сохраняет в файл информацию о сеансе
        /// </summary>
        /// <param name="patient">Пациент, которому принадлежит сеанс</param>
        /// <param name="session">Сеанс пациента</param>
        public static void SaveToFile(Patient patient, Session session)
        {
            if (patient == null) throw new ArgumentNullException("patient");
            if (session == null) throw new ArgumentNullException("session");

            try
            {
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
                var birthDateSring = String.Format("{0}_{1}_{2}", session.DateTime.Day,
                                                                        session.DateTime.Month,
                                                                        session.DateTime.Year);
                var fileName = String.Format("{0}_{1}_{2}_{3}_{4}_{5}.cmsf", patient.LastName,
                                                                         patient.FirstName,
                                                                         patient.PatronymicName,
                                                                         birthDateSring,
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
            catch (Exception ex)
            {
                Logger.Instance.LogError("File repositry", ex);
                throw new Exception(Localisation.FileRepository_SavePatientException);
            }
            
        }

        /// <summary>
        /// Загружает инфомрацию о сеансе из файла
        /// </summary>
        /// <param name="filePath">Полный путь к файлу сеансу</param>
        /// <returns>Сенас</returns>
        public static SessionContainer LoadFromFile(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath)) { throw new ArgumentNullException("filePath"); }

            try
            {
                SessionContainer container;
                using (var loadingStream = new FileStream(filePath, FileMode.Open))
                {
                    var bf = new BinaryFormatter();
                    container = (SessionContainer)bf.Deserialize(loadingStream);
                    loadingStream.Close();
                }
                return container;
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("File repositry", ex);
                throw new Exception(Localisation.FileRepository_LoadPatientException);
            }
        }
    }
}
