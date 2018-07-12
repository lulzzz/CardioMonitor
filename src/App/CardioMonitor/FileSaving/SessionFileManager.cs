using System;
using System.IO;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.FileSaving.Containers;
using CardioMonitor.FileSaving.Containers.V1;
using CardioMonitor.FileSaving.Exceptions;
using CardioMonitor.FileSaving.Mappers.V1;
using CardioMonitor.Resources;
using CardioMonitor.Settings;
using JetBrains.Annotations;
using Markeli.Utils.Logging;
using Newtonsoft.Json;

namespace CardioMonitor.FileSaving 
{
    /// <summary>
    /// Менеджер хранения данныъ сеанса в файле
    /// </summary>
    internal class SessionFileManager : ISessionFileManager
    {
        private const int ActualStoredContainerVersion = 1;
        
        private readonly ILogger _logger;
        private readonly ICardioSettings _settings;

        public SessionFileManager(ILogger logger, ICardioSettings settings)
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
        public void Save(Patient patient, Session session, string filePath = null)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));
            if (session == null) throw new ArgumentNullException(nameof(session));

            string dirPath;
            if (filePath == null)
            {
                if (String.IsNullOrWhiteSpace(_settings.SessionsFilesDirectoryPath))
                    throw new SettingsException("Не задана директория для сохранения сеанса в файл");
                
                filePath = _settings.SessionsFilesDirectoryPath;
                dirPath = $"{patient.LastName}_{patient.FirstName}_{patient.PatronymicName}_{patient.Id}";
                filePath = Path.Combine(filePath, dirPath);


                var dateSring =
                    $"{session.TimestampUtc.Day}_{session.TimestampUtc.Month}_{session.TimestampUtc.Year}_{session.TimestampUtc.Hour}_{session.TimestampUtc.Minute}_{session.TimestampUtc.Second}";
                var birthDateSring = $"{session.TimestampUtc.Day}_{session.TimestampUtc.Month}_{session.TimestampUtc.Year}";
                var fileName =
                    $"{patient.LastName}_{patient.FirstName}_{patient.PatronymicName}_{birthDateSring}_{patient.Id}_{dateSring}.cmsf";
                filePath = Path.Combine(filePath, fileName);
            }
            else
            {
                dirPath = Path.GetDirectoryName(filePath);
            }

            if (String.IsNullOrWhiteSpace(dirPath))
                throw new SavingException("Не задан путь для сохранения сеанса в файл");

            try
            {

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var sessionContainer = new SessionContainer
                {
                    Patient = patient,
                    Session = session
                };

                var storedSessionContainer = sessionContainer.ToStoredV1();

                var storedSessionJson = JsonConvert.SerializeObject(storedSessionContainer);

                var storedDataContainer = new FileStorageDataContainer
                {
                    Version = ActualStoredContainerVersion,
                    DataJson = storedSessionJson
                };
                var storedDataContainerJson = JsonConvert.SerializeObject(storedDataContainer);

                File.WriteAllText(filePath, storedDataContainerJson);
                
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка сохранения сеанса в файл. Причина: {ex.Message}", ex);
                throw new SavingException(Localisation.FileRepository_SavePatientException, ex);
            }
        }

        /// <summary>
        /// Загружает информацию о сеансе из файла
        /// </summary>
        /// <param name="filePath">Полный путь к файлу сеансу</param>
        /// <returns>Сенас</returns>
        public SessionContainer Load(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath)) { throw new ArgumentNullException(nameof(filePath)); }

            try
            {
                var json = File.ReadAllText(filePath);
                var fileStorageContainer = JsonConvert.DeserializeObject<FileStorageDataContainer>(json);
                return GetContainer(fileStorageContainer);
            }
            catch (SavingException ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка загрузки сеанса из файла. Причина: {ex.Message}", ex);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Ошибка загрузки сеанса из файла. Причина: {ex.Message}", ex);
                throw new Exception(Localisation.FileRepository_LoadPatientException);
            }
        }

        private SessionContainer GetContainer([NotNull] FileStorageDataContainer dataContainer)
        {
            switch (dataContainer.Version)
            {
                case 1:
                {
                    var storedSessionContainer =
                        JsonConvert.DeserializeObject<StoredSessionContainerV1>(dataContainer.DataJson);

                    return storedSessionContainer.ToDomain();
                }
                    default:
                        throw new SavingException("Неподдерижваемый формат файла");
            }   
        }
    }
}
