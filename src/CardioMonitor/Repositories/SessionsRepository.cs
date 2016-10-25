using System;
using System.Collections.Generic;
using System.Globalization;
using CardioMonitor.DataBase;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Models.Session;
using CardioMonitor.Repositories.Abstract;
using CardioMonitor.Resources;
using CardioMonitor.Settings;

namespace CardioMonitor.Repositories
{
    internal class SessionsRepository : ISessionsRepository
    {
        private readonly DataBaseFactory _dataBaseFactory;
        private readonly ILogger _logger;
        private readonly ICardioSettings _settings;

        public SessionsRepository(
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
        /// Возвращает информацию о сеансах для указанного курса лечения
        /// </summary>
        /// <param name="treatmentId">Курс лечения</param>
        /// <returns>Информация о сеансах</returns>
        public List<SessionInfo> GetSessionInfos(int treatmentId)
        {
            var query = String.Empty;
            try
            {
                var control = _dataBaseFactory.CreateDataBaseController();
                var output = new List<SessionInfo>();
                query =
                    "SELECT id, DateTime, Status " +
                    $"FROM {_settings.DataBaseSettings.DataBase}.sessions " +
                    $"WHERE TreatmentID='{treatmentId}'";
                var reader = control.ConnectDb(query);
                var safeReader = _dataBaseFactory.CreateSafeReader(reader);

                while (safeReader.CanRead())
                {
                    var patient = new SessionInfo
                    {
                        Id = safeReader.GetInt(0),
                        DateTime = safeReader.GetDateTime(1),
                        Status = (SessionStatus)safeReader.GetInt(2)
                    };
                    output.Add(patient);
                }
                control.DisсonnectDb(reader);
                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(SessionsRepository), ex);
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
                        message = Localisation.DataBaseRepository_SessionInfo_GetException;
                        break;
                }
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Удаляет сеанс
        /// </summary>
        /// <param name="sessionId">Ид сеанса</param>
        public void DeleteSession(int sessionId)
        {
            var query = String.Empty;
            try
            {
                query =
                    $"DELETE FROM {_settings.DataBaseSettings.DataBase}.sessions " +
                    $"WHERE id='{sessionId}'";
                var control = _dataBaseFactory.CreateDataBaseController();
                control.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(SessionsRepository), ex);
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
                        message = Localisation.DataBaseRepository_Session_DeleteException;
                        break;
                }
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Добавляет новый сеанс в базу
        /// </summary>
        /// <param name="session">Сеанс</param>
        public void AddSession(Session session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var query = String.Empty;
            try
            {
                query =
                    $"INSERT INTO {_settings.DataBaseSettings.DataBase}.sessions (TreatmentId, DateTime, Status) " +
                    $"VALUES ('{session.TreatmentId}','{session.DateTime:yyyy-MM-dd HH:mm:ss}','{(int) session.Status}')";
                var control = _dataBaseFactory.CreateDataBaseController();
                control.ExecuteQuery(query);

                //getting id of new 
                query =
                    $"SELECT id FROM {_settings.DataBaseSettings.DataBase}.sessions " +
                    $"WHERE TreatmentId='{session.TreatmentId}' " +
                    $"AND DateTime='{session.DateTime:yyyy-MM-dd HH:mm:ss}' AND Status='{(int) session.Status}'";
                var reader = control.ConnectDb(query);
                var safeReader = _dataBaseFactory.CreateSafeReader(reader);
                safeReader.CanRead();
                var sessionId = safeReader.GetInt(0);
                control.DisсonnectDb(reader);

                foreach (var param in session.PatientParams)
                {
                    const string columns =
                        "Iteration,SessionId,InclinationAngle,HeartRate,RepsirationRate,Spo2,SystolicArterialPressure,DiastolicArterialPressure,AverageArterialPressure";
                    query =
                        $"INSERT INTO {_settings.DataBaseSettings.DataBase}.params ({columns}) " +
                        $"VALUES ('{param.Iteraton}','{sessionId}','{param.InclinationAngle.ToString(CultureInfo.GetCultureInfoByIetfLanguageTag("en"))}','{param.HeartRate}','{param.RepsirationRate}','{param.Spo2}','{param.SystolicArterialPressure}','{param.DiastolicArterialPressure}','{param.AverageArterialPressure}')";
                    control.ExecuteQuery(query);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(SessionsRepository), ex);
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
                        message = Localisation.DataBaseRepository_Session_AddException;
                        break;
                }
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Возвращает информацию об указанном сеанса
        /// </summary>
        /// <param name="sessionId">Ид сеанса</param>
        /// <returns>Сеанс</returns>
        public Session GetSession(int sessionId)
        {
            var query = String.Empty;
            try
            {
                query = $"SELECT * FROM {_settings.DataBaseSettings.DataBase}.sessions " +
                        $"WHERE id='{sessionId}'";
                var control = _dataBaseFactory.CreateDataBaseController();

                var reader = control.ConnectDb(query);
                var safeReader = _dataBaseFactory.CreateSafeReader(reader);

                var session = new Session();
                while (safeReader.CanRead())
                {
                    session.Id = safeReader.GetInt(0);
                    session.TreatmentId = safeReader.GetInt(1);
                    session.DateTime = safeReader.GetDateTime(2);
                    session.Status = safeReader.GetSessionStatus(3);
                }
                control.DisсonnectDb(reader);

                query =
                    $"SELECT * FROM {_settings.DataBaseSettings.DataBase}.params " +
                    $"WHERE SessionId='{sessionId}'";
                reader = control.ConnectDb(query);
                safeReader = _dataBaseFactory.CreateSafeReader(reader);

                while (safeReader.CanRead())
                {
                    var param = new PatientParams
                    {
                        Id = safeReader.GetInt(0),
                        Iteraton = safeReader.GetInt(1),
                        SessionId = safeReader.GetInt(2),
                        InclinationAngle = safeReader.GetDouble(3),
                        HeartRate = safeReader.GetInt(4),
                        RepsirationRate = safeReader.GetInt(5),
                        Spo2 = safeReader.GetInt(6),
                        SystolicArterialPressure = safeReader.GetInt(7),
                        DiastolicArterialPressure = safeReader.GetInt(8),
                        AverageArterialPressure = safeReader.GetInt(9)
                    };
                    session.PatientParams.Add(param);
                }
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(SessionsRepository), ex);
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
                        message = Localisation.DataBaseRepository_Session_GetException;
                        break;
                }
                throw new Exception(message);
            }
        }
    }
}