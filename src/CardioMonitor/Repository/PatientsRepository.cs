using System;
using System.Collections.Generic;
using CardioMonitor.DataBase;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Models.Patients;
using CardioMonitor.Resources;
using MySql.Data.MySqlClient;

namespace CardioMonitor.Repository
{
    public class PatientsRepository
    {
        private readonly DataBaseFactory _dataBaseFactory;
        private readonly ILogger _logger;

        public PatientsRepository(
            DataBaseFactory dataBaseFactory,
            ILogger logger)
        {
            if (dataBaseFactory == null) throw new ArgumentNullException(nameof(dataBaseFactory));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _dataBaseFactory = dataBaseFactory;
            _logger = logger;
        }


        /// <summary>
        /// Возвращает список пациентов
        /// </summary>
        /// <returns>Список пациентов</returns>
        public List<Patient> GetPatients()
        {
            var query = String.Empty;
            try
            {
                var control = _dataBaseFactory.CreateDataBaseController();
                var output = new List<Patient>();

                query =
                    "SELECT id, FirstName, PatronymicName, LastName, BirthDate " +
                    $"FROM {Settings.Settings.Instance.DataBase.DataBase}.patients";
                var reader = control.ConnectDb(query);
                var sReader = _dataBaseFactory.CreateSafeReader(reader);

                while (sReader.CanRead())
                {
                    var patient = new Patient
                    {
                        Id = sReader.GetInt(0),
                        FirstName = sReader.GetString(1),
                        PatronymicName = sReader.GetString(2),
                        LastName = sReader.GetString(3),
                        BirthDate = sReader.GetNullableDateTime(4)
                    };
                    output.Add(patient);
                }
                control.DisсonnectDb(reader);
                return output;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(nameof(PatientsRepository), ex);
                _logger.LogQueryError(query);
                var translator = _dataBaseFactory.CreateDataBaseErrorTranslator();
                var error = translator.Translate(ex);
                string message;
                switch (error)
                {
                    case DataBaseError.HostError:
                        message = Localisation.DataBaseRepository_HostAccessException;
                        break;
                    case DataBaseError.AccessDenied:
                        message = Localisation.DataBaseRepository_AccessDeniedException;
                        break;
                    default:
                        message = Localisation.DataBaseRepository_Patients_GetExcepttion;
                        break;
                }
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Добавляет пациента в базу
        /// </summary>
        /// <param name="patient">Пациент</param>
        public void AddPatient(Patient patient)
        {
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }

            var query = String.Empty;
            try
            {
                var patientBirthDate = patient.BirthDate ?? new DateTime();
                query =
                    $"INSERT INTO {Settings.Settings.Instance.DataBase.DataBase}.patients " +
                    "(LastName,FirstName,PatronymicName, BirthDate)" +
                    $" VALUES ('{patient.LastName}','{patient.FirstName}','{patient.PatronymicName}', '{patientBirthDate:yyyy-MM-dd H:mm:ss}')";
                var control = _dataBaseFactory.CreateDataBaseController();
                control.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(PatientsRepository), ex);
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
                        message = Localisation.DataBaseRepository_Patient_AddExcepttion;
                        break;
                }
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Обновляет информацию о пациента
        /// </summary>
        /// <param name="patient">Пациент, информацию о котором необходимо обновить</param>
        public void UpdatePatient(Patient patient)
        {
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }

            var query = String.Empty;
            try
            {
                var patientBirthDate = patient.BirthDate ?? new DateTime();
                query =
                    $"UPDATE {Settings.Settings.Instance.DataBase.DataBase}.patients " +
                    $"SET LastName='{patient.LastName}', FirstName='{patient.FirstName}', " +
                        $"PatronymicName='{patient.PatronymicName}', BirthDate='{patientBirthDate:yyyy-MM-dd H:mm:ss}' " +
                    $"WHERE id='{patient.Id}'";

                var control = _dataBaseFactory.CreateDataBaseController();
                control.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(PatientsRepository), ex);
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
                        message = Localisation.DataBaseRepository_Patient_UpdateExcepttion;
                        break;
                }
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Удаляет выбранного пациента
        /// </summary>
        /// <param name="patientId">Ид пациента</param>
        public void DeletePatient(int patientId)
        {
            var query = String.Empty;
            try
            {
                query =
                    $"DELETE FROM {Settings.Settings.Instance.DataBase.DataBase}.patients WHERE id='{patientId}'";

                var control = _dataBaseFactory.CreateDataBaseController();
                control.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(PatientsRepository), ex);
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
                        message = Localisation.DataBaseRepository_Patient_DeleteExcepttion;
                        break;
                }
                throw new Exception(message);
            }
        }
    }
}