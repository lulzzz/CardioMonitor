using System;
using System.Collections.ObjectModel;
using CardioMonitor.Core.Models.Patients;
using CardioMonitor.Core.Models.Session;
using CardioMonitor.Core.Models.Treatment;
using CardioMonitor.Patients;

namespace CardioMonitor.Core.Repository.DataBase
{
    public class DataBaseRepository
    {
        private static DataBaseRepository _instance;
        private static readonly object SyncObject = new object();

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
                    if (null == _instance)
                    {
                        _instance = new DataBaseRepository();
                    }
                }
                return _instance;
            }
        }

        public ObservableCollection<Patient> GetPatients()
        {
            try
            {
                var control = new DataBaseController();
                var output = new ObservableCollection<Patient>();

                var queryMain = String.Format("SELECT id, FirstName, PatronymicName, LastName FROM {0}.patients",
                    Settings.Settings.Instance.DataBase.DataBase);
                var reader = control.ConnectDB(queryMain);
                var sreader = new SafeReader(reader);

                while (reader.Read())
                {
                    var patient = new Patient
                    {
                        Id = sreader.GetInt(0),
                        FirstName = sreader.GetString(1),
                        PatronymicName = sreader.GetString(2),
                        LastName = sreader.GetString(3)
                    };
                    output.Add(patient);
                }
                control.DisConnectDB(reader);
                return output;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void AddPatient(Patient patient)
        {
            try
            {
                var query =
                    String.Format(
                        "INSERT INTO {0}.patients (LastName,FirstName,PatronymicName) VALUES ('{1}','{2}','{3}')",
                        Settings.Settings.Instance.DataBase.DataBase,
                        patient.LastName, patient.FirstName, patient.PatronymicName);
                var control = new DataBaseController();
                control.ExecuteQuery(query);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdatePatient(Patient patient)
        {
            try
            {
                var query =
                    String.Format(
                        "UPDATE {0}.patients SET LastName='{1}', FirstName='{2}', PatronymicName='{3}' WHERE id='{4}'",
                        Settings.Settings.Instance.DataBase.DataBase,
                        patient.LastName, patient.FirstName, patient.PatronymicName, patient.Id);

                var control = new DataBaseController();
                control.ExecuteQuery(query);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeletePatient(int patientId)
        {
            try
            {
                var query =
                    String.Format(
                        "DELETE FROM {0}.patients WHERE id='{1}'", Settings.Settings.Instance.DataBase.DataBase,
                        patientId);

                var control = new DataBaseController();
                control.ExecuteQuery(query);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ObservableCollection<Treatment> GetTreatments(int patientId)
        {
            try
            {
                var control = new DataBaseController();
                var output = new ObservableCollection<Treatment>();
                var queryMain = String.Format("SELECT * FROM {0}.treatments WHERE PatientId='{1}'",
                    Settings.Settings.Instance.DataBase.DataBase, patientId);
                var reader = control.ConnectDB(queryMain);
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
            catch (Exception)
            {
                throw;
            }
        }

        public void AddTreatment(Treatment treatment)
        {
            try
            {
                var query =
                    String.Format(
                        "INSERT INTO {0}.treatments (PatientId,StartDate) VALUES ('{1}','{2}')",
                        Settings.Settings.Instance.DataBase.DataBase,
                        treatment.PatientId, treatment.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
                var control = new DataBaseController();
                control.ExecuteQuery(query);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteTreatment(int treatmentId)
        {
            try
            {
                var query =
                    String.Format(
                        "DELETE FROM {0}.treatments WHERE id='{1}'", Settings.Settings.Instance.DataBase.DataBase,
                        treatmentId);

                var control = new DataBaseController();
                control.ExecuteQuery(query);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ObservableCollection<SessionInfo> GetSessionInfos(int treatmentId)
        {
            try
            {
                var control = new DataBaseController();
                var output = new ObservableCollection<SessionInfo>();
                var queryMain = String.Format("SELECT id, DateTime, Status FROM {0}.sessions WHERE TreatmentID='{1}'",
                    Settings.Settings.Instance.DataBase.DataBase, treatmentId);
                var reader = control.ConnectDB(queryMain);
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
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteSession(int sessionId)
        {
            try
            {
                var query =
                    String.Format(
                        "DELETE FROM {0}.sessions WHERE id='{1}'", Settings.Settings.Instance.DataBase.DataBase,
                        sessionId);
                var control = new DataBaseController();
                control.ExecuteQuery(query);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void AddSession(Session session)
        {
            try
            {
                var query =
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
                            columns, param.Iteraton, sessionId, param.InclinationAngle, param.HeartRate,
                            param.RepsirationRate, param.Spo2, param.SystolicArterialPressure,
                            param.DiastolicArterialPressure, param.AverageArterialPressure);
                    control.ExecuteQuery(query);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Session GetSession(int sessionId)
        {
            try
            {
                var query = String.Format("SELECT * FROM {0}.sessions WHERE id='{1}'", Settings.Settings.Instance.DataBase.DataBase, sessionId);
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

                query = String.Format("SELECT * FROM {0}.params WHERE SessionId='{1}'", Settings.Settings.Instance.DataBase.DataBase, sessionId);
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
            catch (Exception)
            {
                throw;
            }
        }

    }
}
