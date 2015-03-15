using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardioMonitor.Patients;
using MySql.Data.MySqlClient;

namespace CardioMonitor.Core.Repository
{
    public class Repository
    {
        private static Repository _instance;
        private static readonly object _syncObject = new object();

        public static Repository Instance
        {
            get
            {
                if (null != _instance)
                {
                    return _instance;
                }
                lock (_syncObject)
                {
                    if (null == _instance)
                    {
                        return _instance = new Repository();
                    }
                }
                return _instance;
            }
        }


        public ObservableCollection<Patient> GetPatients()
        {
            try
            {
                var cntrl = new DataBaseController();
                var output = new ObservableCollection<Patient>();
                //string QueryMain = "SELECT id, FirstName, MidName, LastName FROM " + MvcApplication.Config["Database"] + ".client_main;";
                var queryMain = String.Format("SELECT id, FirstName, PatronymicName, LastName FROM {0}.patients",
                    Settings.Settings.Instance.DataBase.DataBase);
                var reader = cntrl.ConnectDB(queryMain);
                var sreader = new SafeReader(reader);

                while (reader.Read())
                {
                    var patient = new Patient
                    {
                        Id = sreader.SafeGetInt(0),
                        FirstName = sreader.SafeGetString(1),
                        PatronymicName = sreader.SafeGetString(2),
                        LastName = sreader.SafeGetString(3)
                    };
                    output.Add(patient);
                }
                cntrl.DisConnectDB(reader);
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
                var temp = Encoding.Default;
                var query =
                    String.Format(
                        "INSERT INTO {0}.patients (LastName,FirstName,PatronymicName) VALUES ('{1}','{2}','{3}')", Settings.Settings.Instance.DataBase.DataBase,
                        patient.LastName, patient.FirstName, patient.PatronymicName);
                var cntrl = new DataBaseController();
                cntrl.ExecuteDB(query);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string EncodeForSql(string query)
        {

            var bytes = Encoding.Default.GetBytes(query);
            bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, bytes);
            return Encoding.UTF8.GetString(bytes);
        }

        public string EncodeFomSql(string query)
        {
            var bytes = Encoding.Default.GetBytes(query);
            bytes = Encoding.Convert(Encoding.UTF8, Encoding.Default, bytes);
            return Encoding.Default.GetString(bytes);
        }

        public void UpdatePatient(Patient patient)
        {
            try
            {
                var query =
                    String.Format(
                        "UPDATE {0}.patients SET LastName='{1}', FirstName='{2}', PatronymicName='{3}' WHERE id='{4}'", Settings.Settings.Instance.DataBase.DataBase,
                        patient.LastName, patient.FirstName, patient.PatronymicName, patient.Id);

                var cntrl = new DataBaseController();
                cntrl.ExecuteDB(query);
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
                        "DELETE FROM {0}.patients WHERE id='{1}'", Settings.Settings.Instance.DataBase.DataBase, patientId);

                var cntrl = new DataBaseController();
                cntrl.ExecuteDB(query);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
