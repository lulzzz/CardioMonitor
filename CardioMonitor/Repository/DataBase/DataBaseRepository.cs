using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using CardioMonitor.Logs;
using CardioMonitor.Models.Patients;
using CardioMonitor.Models.Session;
using CardioMonitor.Models.Treatment;
using CardioMonitor.Resources;
using MySql.Data.MySqlClient;

namespace CardioMonitor.Repository.DataBase
{
    /// <summary>
    /// Репозиторий для доступа к базе данных
    /// </summary>
    public class DataBaseRepository
    {
        private static DataBaseRepository _instance;
        private static readonly object SyncObject = new object();

        private const int HostError = 1042;
        private const int AccessDeniedError = 0;

        /// <summary>
        /// Репозиторий для доступа к базе данных
        /// </summary>
        public static DataBaseRepository Instance
        {
            get
            {
                if (null != _instance)
                {
                    return _instance;
                }
                lock (SyncObject)
                {

                    return _instance ?? (_instance = new DataBaseRepository());
                }
            }
        }

        #region Методы для пациентов

        /// <summary>
        /// Возвращает список пациентов
        /// </summary>
        /// <returns>Список пациентов</returns>
        public ObservableCollection<Patient> GetPatients()
        {
            var query = "";
            try
            {
                var control = new DataBaseController();
                var output = new ObservableCollection<Patient>();

                query = String.Format("SELECT id, FirstName, PatronymicName, LastName, BirthDate FROM {0}.patients",
                    Settings.Settings.Instance.DataBase.DataBase);
                var reader = control.ConnectDB(query);
                var sreader = new SafeReader(reader);

                while (reader.Read())
                {
                    var patient = new Patient
                    {
                        Id = sreader.GetInt(0),
                        FirstName = sreader.GetString(1),
                        PatronymicName = sreader.GetString(2),
                        LastName = sreader.GetString(3),
                        BirthDate = sreader.GetNullableDateTime(4)
                    };
                    output.Add(patient);
                }
                control.DisConnectDB(reader);
                return output;
            }
            catch (MySqlException ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                string message;
                switch (ex.Number)
                {
                    case HostError:
                        message = Localisation.DataBaseRepository_HostAccessException;
                        break;
                    case AccessDeniedError:
                        message = Localisation.DataBaseRepository_AccessDeniedException;
                        break;
                    default:
                        message = Localisation.DataBaseRepository_Patients_GetExcepttion;
                        break;
                }
                throw new Exception(message);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                throw new Exception(Localisation.DataBaseRepository_Patients_GetExcepttion);
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
                throw new ArgumentNullException("patient");
            }

            var query = "";
            try
            {
                var patientBirthDate = patient.BirthDate.HasValue ? patient.BirthDate.Value : new DateTime();
                query =
                    String.Format(
                        "INSERT INTO {0}.patients (LastName,FirstName,PatronymicName, BirthDate) VALUES ('{1}','{2}','{3}', '{4}')",
                        Settings.Settings.Instance.DataBase.DataBase,
                        patient.LastName, patient.FirstName, patient.PatronymicName, patientBirthDate.ToString("yyyy-MM-dd H:mm:ss"));
                var control = new DataBaseController();
                control.ExecuteQuery(query);
            }
            catch (MySqlException ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                string message;
                switch (ex.Number)
                {
                    case HostError:
                        message = Localisation.DataBaseRepository_HostAccessException;
                        break;
                    case AccessDeniedError:
                        message = Localisation.DataBaseRepository_AccessDeniedException;
                        break;
                    default:
                        message = Localisation.DataBaseRepository_Patient_AddExcepttion;
                        break;
                }
                throw new Exception(message);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                throw new Exception(Localisation.DataBaseRepository_Patient_AddExcepttion);
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
                throw new ArgumentNullException("patient");
            }

            var query = "";
            try
            {
                var patientBirthDate = patient.BirthDate.HasValue ? patient.BirthDate.Value : new DateTime();
                query =
                    String.Format(
                        "UPDATE {0}.patients SET LastName='{1}', FirstName='{2}', PatronymicName='{3}', BirthDate='{4}' WHERE id='{5}'",
                        Settings.Settings.Instance.DataBase.DataBase,
                        patient.LastName, patient.FirstName, patient.PatronymicName, patientBirthDate.ToString("yyyy-MM-dd H:mm:ss"), patient.Id);

                var control = new DataBaseController();
                control.ExecuteQuery(query);
            }
            catch (MySqlException ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                string message;
                switch (ex.Number)
                {
                    case HostError:
                        message = Localisation.DataBaseRepository_HostAccessException;
                        break;
                    case AccessDeniedError:
                        message = Localisation.DataBaseRepository_AccessDeniedException;
                        break;
                    default:
                        message = Localisation.DataBaseRepository_Patient_UpdateExcepttion;
                        break;
                }
                throw new Exception(message);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                throw new Exception(Localisation.DataBaseRepository_Patient_UpdateExcepttion);
            }
        }

        /// <summary>
        /// Удаляет выбранного пациента
        /// </summary>
        /// <param name="patientId">Ид пациента</param>
        public void DeletePatient(int patientId)
        {
            var query = "";
            try
            {
                query =
                    String.Format(
                        "DELETE FROM {0}.patients WHERE id='{1}'", Settings.Settings.Instance.DataBase.DataBase,
                        patientId);

                var control = new DataBaseController();
                control.ExecuteQuery(query);
            }
            catch (MySqlException ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                string message;
                switch (ex.Number)
                {
                    case HostError:
                        message = Localisation.DataBaseRepository_HostAccessException;
                        break;
                    case AccessDeniedError:
                        message = Localisation.DataBaseRepository_AccessDeniedException;
                        break;
                    default:
                        message = Localisation.DataBaseRepository_Patient_DeleteExcepttion;
                        break;
                }
                throw new Exception(message);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                throw new Exception(Localisation.DataBaseRepository_Patient_DeleteExcepttion);
            }
        }

        #endregion

        #region Методы для курса лечения

        /// <summary>
        /// Возвращает курсы лечения для выбранногоп пациента
        /// </summary>
        /// <param name="patientId">ИД пациента</param>
        /// <returns>Курсы лечения</returns>
        public ObservableCollection<Treatment> GetTreatments(int patientId)
        {
            var query = "";
            try
            {
                var control = new DataBaseController();
                var output = new ObservableCollection<Treatment>();
                query = String.Format("SELECT * FROM {0}.treatments WHERE PatientId='{1}'",
                    Settings.Settings.Instance.DataBase.DataBase, patientId);
                var reader = control.ConnectDB(query);
                var sreader = new SafeReader(reader);

                while (reader.Read())
                {
                    var patient = new Treatment
                    {
                        Id = sreader.GetInt(0),
                        PatientId = sreader.GetInt(1),
                        StartDate = sreader.GetDateTime(2),
                        LastSessionDate = sreader.GetDateTime(3),
                        SessionsCount = sreader.GetInt(4)
                    };
                    output.Add(patient);
                }
                control.DisConnectDB(reader);
                return output;
            }
            catch (MySqlException ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                string message;
                switch (ex.Number)
                {
                    case HostError:
                        message = Localisation.DataBaseRepository_HostAccessException;
                        break;
                    case AccessDeniedError:
                        message = Localisation.DataBaseRepository_AccessDeniedException;
                        break;
                    default:
                        message = Localisation.DataBaseRepository_Treatments_GetException;
                        break;
                }
                throw new Exception(message);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                throw new Exception(Localisation.DataBaseRepository_Treatments_GetException);
            }
        }

        /// <summary>
        /// Добавялет новый курс лечения в базу
        /// </summary>
        /// <param name="treatment">Курс лечения</param>
        public void AddTreatment(Treatment treatment)
        {
            if (treatment == null)
            {
                throw new ArgumentNullException("treatment");
            }

            var query = "";
            try
            {
                query =
                    String.Format(
                        "INSERT INTO {0}.treatments (PatientId,StartDate) VALUES ('{1}','{2}')",
                        Settings.Settings.Instance.DataBase.DataBase,
                        treatment.PatientId, treatment.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
                var control = new DataBaseController();
                control.ExecuteQuery(query);
            }
            catch (MySqlException ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                string message;
                switch (ex.Number)
                {
                    case HostError:
                        message = Localisation.DataBaseRepository_HostAccessException;
                        break;
                    case AccessDeniedError:
                        message = Localisation.DataBaseRepository_AccessDeniedException;
                        break;
                    default:
                        message = Localisation.DataBaseRepository_Treatment_AddException;
                        break;
                }
                throw new Exception(message);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                throw new Exception(Localisation.DataBaseRepository_Treatment_AddException);
            }
        }

        /// <summary>
        /// Удаляет курс лечения
        /// </summary>
        /// <param name="treatmentId">Ид курса лечения</param>
        public void DeleteTreatment(int treatmentId)
        {
            var query = "";
            try
            {
                query =
                    String.Format(
                        "DELETE FROM {0}.treatments WHERE id='{1}'", Settings.Settings.Instance.DataBase.DataBase,
                        treatmentId);

                var control = new DataBaseController();
                control.ExecuteQuery(query);
            }
            catch (MySqlException ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                string message;
                switch (ex.Number)
                {
                    case HostError:
                        message = Localisation.DataBaseRepository_HostAccessException;
                        break;
                    case AccessDeniedError:
                        message = Localisation.DataBaseRepository_AccessDeniedException;
                        break;
                    default:
                        message = Localisation.DataBaseRepository_Treatment_DeleteException;
                        break;
                }
                throw new Exception(message);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                throw new Exception(Localisation.DataBaseRepository_Treatment_DeleteException);
            }
        }

        #endregion

        #region Методы для сессии 

        /// <summary>
        /// Возвращает информацию о сенасах для указанного курса лечения
        /// </summary>
        /// <param name="treatmentId">Курс леченяи</param>
        /// <returns>Информация о сеансах</returns>
        public ObservableCollection<SessionInfo> GetSessionInfos(int treatmentId)
        {
            var query = "";
            try
            {
                var control = new DataBaseController();
                var output = new ObservableCollection<SessionInfo>();
                query = String.Format("SELECT id, DateTime, Status FROM {0}.sessions WHERE TreatmentID='{1}'",
                    Settings.Settings.Instance.DataBase.DataBase, treatmentId);
                var reader = control.ConnectDB(query);
                var sreader = new SafeReader(reader);

                while (reader.Read())
                {
                    var patient = new SessionInfo
                    {
                        Id = sreader.GetInt(0),
                        DateTime = sreader.GetDateTime(1),
                        Status = (SessionStatus) sreader.GetInt(2)
                    };
                    output.Add(patient);
                }
                control.DisConnectDB(reader);
                return output;
            }
            catch (MySqlException ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                string message;
                switch (ex.Number)
                {
                    case HostError:
                        message = Localisation.DataBaseRepository_HostAccessException;
                        break;
                    case AccessDeniedError:
                        message = Localisation.DataBaseRepository_AccessDeniedException;
                        break;
                    default:
                        message = Localisation.DataBaseRepository_SessionInfo_GetException;
                        break;
                }
                throw new Exception(message);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                throw new Exception(Localisation.DataBaseRepository_SessionInfo_GetException);
            }
        }

        /// <summary>
        /// Удаляет сеанс
        /// </summary>
        /// <param name="sessionId">Ид сеанса</param>
        public void DeleteSession(int sessionId)
        {
            var query = "";
            try
            {
                query =
                    String.Format(
                        "DELETE FROM {0}.sessions WHERE id='{1}'", Settings.Settings.Instance.DataBase.DataBase,
                        sessionId);
                var control = new DataBaseController();
                control.ExecuteQuery(query);
            }
            catch (MySqlException ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                string message;
                switch (ex.Number)
                {
                    case HostError:
                        message = Localisation.DataBaseRepository_HostAccessException;
                        break;
                    case AccessDeniedError:
                        message = Localisation.DataBaseRepository_AccessDeniedException;
                        break;
                    default:
                        message = Localisation.DataBaseRepository_Session_DeleteException;
                        break;
                }
                throw new Exception(message);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                throw new Exception(Localisation.DataBaseRepository_Session_DeleteException);
            }
        }

        /// <summary>
        /// Добавляет новый сеанс в базу
        /// </summary>
        /// <param name="session">Сеасн</param>
        public void AddSession(Session session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            var query = "";
            try
            {
                query =
                    String.Format("INSERT INTO {0}.sessions (TreatmentId, DateTime, Status) VALUES ('{1}','{2}','{3}')",
                        Settings.Settings.Instance.DataBase.DataBase, session.TreatmentId,
                        session.DateTime.ToString("yyyy-MM-dd HH:mm:ss"), (int) session.Status);
                var control = new DataBaseController();
                control.ExecuteQuery(query);

                //getting id of new 
                query =
                    String.Format(
                        "SELECT id FROM {0}.sessions WHERE TreatmentId='{1}' AND DateTime='{2}' AND Status='{3}'",
                        Settings.Settings.Instance.DataBase.DataBase, session.TreatmentId,
                        session.DateTime.ToString("yyyy-MM-dd HH:mm:ss"), (int) session.Status);
                var reader = control.ConnectDB(query);
                var sreader = new SafeReader(reader);
                reader.Read();
                var sessionId = sreader.GetInt(0);
                control.DisConnectDB(reader);

                foreach (var param in session.PatientParams)
                {
                    const string columns =
                        "Iteration,SessionId,InclinationAngle,HeartRate,RepsirationRate,Spo2,SystolicArterialPressure,DiastolicArterialPressure,AverageArterialPressure";
                    query =
                        String.Format(
                            "INSERT INTO {0}.params ({1}) VALUES ('{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}')",
                            Settings.Settings.Instance.DataBase.DataBase,
                            columns, param.Iteraton, sessionId,
                            param.InclinationAngle.ToString(CultureInfo.GetCultureInfoByIetfLanguageTag("en")),
                            param.HeartRate,
                            param.RepsirationRate, param.Spo2, param.SystolicArterialPressure,
                            param.DiastolicArterialPressure, param.AverageArterialPressure);
                    control.ExecuteQuery(query);
                }
            }
            catch (MySqlException ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                string message;
                switch (ex.Number)
                {
                    case HostError:
                        message = Localisation.DataBaseRepository_HostAccessException;
                        break;
                    case AccessDeniedError:
                        message = Localisation.DataBaseRepository_AccessDeniedException;
                        break;
                    default:
                        message = Localisation.DataBaseRepository_Session_AddException;
                        break;
                }
                throw new Exception(message);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                throw new Exception(Localisation.DataBaseRepository_Session_AddException);
            }
        }

        /// <summary>
        /// Возвращает информацию об указанном сеанса
        /// </summary>
        /// <param name="sessionId">Ид сеанса</param>
        /// <returns>Сеанс</returns>
        public Session GetSession(int sessionId)
        {
            var query = "";
            try
            {
                query = String.Format("SELECT * FROM {0}.sessions WHERE id='{1}'",
                    Settings.Settings.Instance.DataBase.DataBase, sessionId);
                var control = new DataBaseController();

                var reader = control.ConnectDB(query);
                var sreader = new SafeReader(reader);

                var session = new Session();
                while (reader.Read())
                {
                    session.Id = sreader.GetInt(0);
                    session.TreatmentId = sreader.GetInt(1);
                    session.DateTime = sreader.GetDateTime(2);
                    session.Status = sreader.GetSesionStatus(3);
                }
                control.DisConnectDB(reader);

                query = String.Format("SELECT * FROM {0}.params WHERE SessionId='{1}'",
                    Settings.Settings.Instance.DataBase.DataBase, sessionId);
                reader = control.ConnectDB(query);
                sreader = new SafeReader(reader);

                while (reader.Read())
                {
                    var param = new PatientParams
                    {
                        Id = sreader.GetInt(0),
                        Iteraton = sreader.GetInt(1),
                        SessionId = sreader.GetInt(2),
                        InclinationAngle = sreader.GetDouble(3),
                        HeartRate = sreader.GetInt(4),
                        RepsirationRate = sreader.GetInt(5),
                        Spo2 = sreader.GetInt(6),
                        SystolicArterialPressure = sreader.GetInt(7),
                        DiastolicArterialPressure = sreader.GetInt(8),
                        AverageArterialPressure = sreader.GetInt(9)
                    };
                    session.PatientParams.Add(param);
                }
                return session;
            }
            catch (MySqlException ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                string message;
                switch (ex.Number)
                {
                    case HostError:
                        message = Localisation.DataBaseRepository_HostAccessException;
                        break;
                    case AccessDeniedError:
                        message = Localisation.DataBaseRepository_AccessDeniedException;
                        break;
                    default:
                        message = Localisation.DataBaseRepository_Session_GetException;
                        break;
                }
                throw new Exception(message);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("DataBaseRepository", ex);
                Logger.Instance.LogQueryError(query);
                throw new Exception(Localisation.DataBaseRepository_Session_GetException);
            }
        }

        #endregion

        #region Методы для получения информации о базе

        /// <summary>
        /// Возвращает статус соединения
        /// </summary>
        /// <returns></returns>
        public bool GetConnectionStatus()
        {
            var control = new DataBaseController();
            return control.GetConnectionStatus();
        }

        /// <summary>
        /// Проверяте соединение к базе
        /// </summary>
        /// <remarks>
        /// Может выбросить эксепшн с информацией о проблеме
        /// </remarks>
        public async Task CheckConnectionAsync(string dataBase, string source, string user, string password)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    var control = new DataBaseController(dataBase, source, user, password);
                    control.CheckConnection();
                }
                catch (MySqlException ex)
                {
                    Logger.Instance.LogError("DataBaseRepository", ex);
                    string message;
                    switch (ex.Number)
                    {
                        case HostError:
                            message = Localisation.DataBaseRepository_HostAccessException;
                            break;
                        case AccessDeniedError:
                            message = Localisation.DataBaseRepository_AccessDeniedException;
                            break;
                        default:
                            message = Localisation.DataBaseRepository_UnknownException;
                            break;
                    }
                    throw new Exception(message);
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogError("DataBaseRepository", ex);
                    throw new Exception(Localisation.DataBaseRepository_UnknownException);
                }
            });

        }

        #endregion
    }
}
