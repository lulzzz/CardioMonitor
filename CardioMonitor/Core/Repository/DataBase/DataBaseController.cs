using System;
using CardioMonitor.Logs;
using MySql.Data.MySqlClient;

namespace CardioMonitor.Core.Repository.DataBase
{
    /// <summary>
    /// Контроллер для непосредственного взаимодействия с базой данных
    /// </summary>
    internal class DataBaseController
    {
        private static MySqlConnection _myConnect;
        private bool _isOpen;
        
        /// <summary>
        /// Контроллер для непосредственного взаимодействия с базой данных
        /// </summary>
        public DataBaseController()
        {
            var initComand = "Database=" + Settings.Settings.Instance.DataBase.DataBase + ";Data Source=" +
                             Settings.Settings.Instance.DataBase.Source +
                             ";User Id=" + Settings.Settings.Instance.DataBase.User + ";Password=" +Settings.Settings.Instance.DataBase.Password + ";Allow Zero Datetime=True;Convert Zero Datetime=True;charset=utf8";
            _myConnect =  new MySqlConnection(initComand);
            _isOpen = false;

        }

        /// <summary>
        /// Контроллер для непосредственного взаимодействия с базой данных
        /// </summary>
        public DataBaseController(string dataBase, string source, string user, string password)
        {
            var initComand = "Database=" + dataBase + ";Data Source=" +
                             source + ";User Id=" + user + ";Password=" + password + ";Allow Zero Datetime=True;Convert Zero Datetime=True;charset=utf8";
            _myConnect = new MySqlConnection(initComand);
            _isOpen = false;

        }

        /// <summary>
        /// Открывает соединение к базе данных, выполняя переданный запрос, возвращает MySqlDataReader для чтения результатов
        /// </summary>
        /// <param name="query">SQL запрос</param>
        /// <returns>MySqlDataReader для чтения результатов</returns>
        /// <remarks>Открывает соединение с базой данных. Пока его не закрыть, другие запросы выплоняться не будут</remarks>
        /// <remarks>Использовать для запросов типа Select</remarks>
        public MySqlDataReader ConnectDB(string query)
        {
            if (_isOpen)
            {
                throw new AccessViolationException();
            }
            _isOpen = true;
            var cmd = new MySqlCommand(query, _myConnect);
            _myConnect.Open();
            var reader = cmd.ExecuteReader();
            return reader;
        }

        /// <summary>
        /// Закрывает соединие с базой данных
        /// </summary>
        /// <param name="reader">MySqlDataReader, содержащий открытое соединение</param>
        public void DisConnectDB(MySqlDataReader reader)
        {
            if (reader == null) { throw new ArgumentNullException("reader");}
            _isOpen = false;
            reader.Close();
            _myConnect.Close();
        }

        /// <summary>
        /// Выполняет запрос к базе данных
        /// </summary>
        /// <param name="query">Sql запрос</param>
        /// <remarks>Использовать для запросов типа Insert, Delete, Update, т.е. не возвращающих данных</remarks>
        public void ExecuteQuery(string query)
        {
            var cmd = new MySqlCommand(query, _myConnect);
            _myConnect.Open();
            cmd.ExecuteNonQuery();
            _myConnect.Close();
        }

        /// <summary>
        /// Проверяет соединение с базой данных
        /// </summary>
        /// <returns>Результат проверки</returns>
        public bool GetConnectionStatus()
        {
            try
            {
                _myConnect.Open();
                _myConnect.Close();
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.LogError("DataBaseController", e);
                return false;
            }
        }

        /// <summary>
        /// Проверяет соединение с базой данных
        /// </summary>
        public void CheckConnection()
        {
            _myConnect.Open();
            _myConnect.Close();
        }
    }

}
