using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Resources;
using CardioMonitor.Settings;
using Markeli.Utils.Logging;

namespace CardioMonitor.Files 
{
    /// <summary>
    /// Репозиторий для доступа к файлам
    /// </summary>
    internal class FilesManager : IFilesManager
    {
        private readonly ILogger _logger;
        private readonly ICardioSettings _settings;

        public FilesManager(ILogger logger, ICardioSettings settings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Сохраняет в файл информацию о сеансе
        /// </summary>
        /// <param name="patient">Пациент, которому принадлежит сеанс</param>
        /// <param name="session">Сеанс пациента</param>
        /// <param name="filePath">Полный путь к файлу (по умолчанию создается директория с именем пациента, и файл сохраняется в нее)</param>
        public void SaveToFile(Patient patient, Session session, string filePath = null)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));
            if (session == null) throw new ArgumentNullException(nameof(session));

            var dirPath = _settings.SessionsFilesDirectoryPath;
            if (filePath == null)
            {
                filePath = _settings.SessionsFilesDirectoryPath;
                dirPath = $"{patient.LastName}_{patient.FirstName}_{patient.PatronymicName}_{patient.Id}";
                filePath = Path.Combine(filePath, dirPath);


                var dateSring =
                    $"{session.DateTimeUtc.Day}_{session.DateTimeUtc.Month}_{session.DateTimeUtc.Year}_{session.DateTimeUtc.Hour}_{session.DateTimeUtc.Minute}_{session.DateTimeUtc.Second}";
                var birthDateSring = $"{session.DateTimeUtc.Day}_{session.DateTimeUtc.Month}_{session.DateTimeUtc.Year}";
                var fileName =
                    $"{patient.LastName}_{patient.FirstName}_{patient.PatronymicName}_{birthDateSring}_{patient.Id}_{dateSring}.cmsf";
                filePath = Path.Combine(filePath, fileName);
            }
            else
            {
                dirPath = Path.GetDirectoryName(filePath);
            }

            try
            {

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var container = new SessionContainer
                {
                    Patient = patient,
                    Session = session
                };

                using (var savingStream = new FileStream(filePath, FileMode.Create))
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(savingStream, container);
                    savingStream.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка сохранения сеанса в файл. Причина: {ex.Message}", ex);
                throw new Exception(Localisation.FileRepository_SavePatientException);
            }
            
        }

        /// <summary>
        /// Загружает информацию о сеансе из файла
        /// </summary>
        /// <param name="filePath">Полный путь к файлу сеансу</param>
        /// <returns>Сенас</returns>
        public SessionContainer LoadFromFile(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath)) { throw new ArgumentNullException(nameof(filePath)); }

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
                _logger.Error($"{GetType().Name}: Ошибка загрузки сеанса из файла. Причина: {ex.Message}", ex);
                throw new Exception(Localisation.FileRepository_LoadPatientException);
            }
        }
    }
}
