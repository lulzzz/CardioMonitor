using System;
using System.Collections.Generic;
using CardioMonitor.DataBase;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Models.Treatment;
using CardioMonitor.Repositories.Abstract;
using CardioMonitor.Resources;
using CardioMonitor.Settings;

namespace CardioMonitor.Repositories
{
    internal class TreatmentsRepository : ITreatmentsRepository
    {
        private readonly DataBaseFactory _dataBaseFactory;
        private readonly ILogger _logger;
        private readonly ICardioSettings _settings;

        public TreatmentsRepository(
            DataBaseFactory dataBaseFactory,
            ILogger logger,
            ICardioSettings settings)
        {
            if (dataBaseFactory == null) throw new ArgumentNullException(nameof(dataBaseFactory));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _dataBaseFactory = dataBaseFactory;
            _logger = logger;
            _settings = settings;
        }


        /// <summary>
        /// Возвращает курсы лечения для выбранного пациента
        /// </summary>
        /// <param name="patientId">ИД пациента</param>
        /// <returns>Курсы лечения</returns>
        public List<Treatment> GetTreatments(int patientId)
        {
            var query = String.Empty;
            try
            {
                var control = _dataBaseFactory.CreateDataBaseController();
                var output = new List<Treatment>();
                query =
                    $"SELECT * FROM {_settings.DataBaseSettings.DataBase}.treatments " +
                    $"WHERE PatientId='{patientId}'";
                var reader = control.ConnectDb(query);
                var safeReader = _dataBaseFactory.CreateSafeReader(reader);

                while (safeReader.CanRead())
                {
                    var patient = new Treatment
                    {
                        Id = safeReader.GetInt(0),
                        PatientId = safeReader.GetInt(1),
                        StartDate = safeReader.GetDateTime(2),
                        LastSessionDate = safeReader.GetDateTime(3),
                        SessionsCount = safeReader.GetInt(4)
                    };
                    output.Add(patient);
                }
                control.DisсonnectDb(reader);
                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(TreatmentsRepository), ex);
                _logger.LogQueryError(query);
                string message;
                var translator = _dataBaseFactory.CreateDataBaseErrorTranslator();
                var error = translator.Translate(ex);
                switch (error)
                {
                    case DataBaseError.HostError:
                        message = Localisation.DataBaseRepository_HostAccessException;
                        break;
                    case DataBaseError.AccessDenied:
                        message = Localisation.DataBaseRepository_AccessDeniedException;
                        break;
                    default:
                        message = Localisation.DataBaseRepository_Treatments_GetException;
                        break;
                }
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Добавляет новый курс лечения в базу
        /// </summary>
        /// <param name="treatment">Курс лечения</param>
        public void AddTreatment(Treatment treatment)
        {
            if (treatment == null)
            {
                throw new ArgumentNullException(nameof(treatment));
            }

            var query = String.Empty;
            try
            {
                query =
                    $"INSERT INTO {_settings.DataBaseSettings.DataBase}.treatments (PatientId,StartDate) " +
                    $"VALUES ('{treatment.PatientId}','{treatment.StartDate:yyyy-MM-dd HH:mm:ss}')";
                var control = _dataBaseFactory.CreateDataBaseController();
                control.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(TreatmentsRepository), ex);
                _logger.LogQueryError(query);
                string message;
                var translator = _dataBaseFactory.CreateDataBaseErrorTranslator();
                var error = translator.Translate(ex);
                switch (error)
                {
                    case DataBaseError.HostError:
                        message = Localisation.DataBaseRepository_HostAccessException;
                        break;
                    case DataBaseError.AccessDenied:
                        message = Localisation.DataBaseRepository_AccessDeniedException;
                        break;
                    default:
                        message = Localisation.DataBaseRepository_Treatment_AddException;
                        break;
                }
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Удаляет курс лечения
        /// </summary>
        /// <param name="treatmentId">ИД курса лечения</param>
        public void DeleteTreatment(int treatmentId)
        {
            var query = String.Empty;
            try
            {
                query =
                    $"DELETE FROM {_settings.DataBaseSettings.DataBase}.treatments WHERE id='{treatmentId}'";

                var control = _dataBaseFactory.CreateDataBaseController();
                control.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(TreatmentsRepository), ex);
                _logger.LogQueryError(query);
                string message;
                var translator = _dataBaseFactory.CreateDataBaseErrorTranslator();
                var error = translator.Translate(ex);
                switch (error)
                {
                    case DataBaseError.HostError:
                        message = Localisation.DataBaseRepository_HostAccessException;
                        break;
                    case DataBaseError.AccessDenied:
                        message = Localisation.DataBaseRepository_AccessDeniedException;
                        break;
                    default:
                        message = Localisation.DataBaseRepository_Treatment_DeleteException;
                        break;
                }
                throw new Exception(message);
            }
        }
    }
}