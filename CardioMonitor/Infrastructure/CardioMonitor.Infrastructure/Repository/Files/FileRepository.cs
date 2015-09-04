using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Infrastructure.Models.Patients;
using CardioMonitor.Infrastructure.Models.Session;
using CardioMonitor.Infrastructure.Resources;

namespace CardioMonitor.Infrastructure.Repository.Files
{
    /// <summary>
    /// Репозиторий для доступа к файлам
    /// </summary>
    public static class FileRepository
    {
        /// <summary>
        /// Сохраняет в файл информацию о сеансе
        /// </summary>
        /// <param name="fileDirectoryPath">Полный путь к корневому катологу хранилища</param>
        /// <param name="patient">Пациент, которому принадлежит сеанс</param>
        /// <param name="session">Сеанс пациента</param>
        public static void SaveToFile(string fileDirectoryPath, Patient patient, Session session)
        {
            if (patient == null) throw new ArgumentNullException("patient");
            if (session == null) throw new ArgumentNullException("session");

            try
            {
                var patientDirPath = String.Format("{0}_{1}_{2}_{3}", patient.LastName, patient.FirstName, patient.PatronymicName,
                patient.Id);
                patientDirPath = Path.Combine(fileDirectoryPath, patientDirPath);
                if (!Directory.Exists(patientDirPath))
                {
                    Directory.CreateDirectory(patientDirPath);
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

                using (var savingStream = new FileStream(Path.Combine(patientDirPath, fileName), FileMode.Create))
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
