using System;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Settings;
using MySql.Data.MySqlClient;

namespace CardioMonitor.DataBase.MySql
{
    /// <summary>
    /// Контроллер для непосредственного взаимодействия с базой данных
    /// </summary>
    internal class MySqlDataBaseController : IDataBaseController
    {
        private readonly ILogger _logger;

        private static MySqlConnection _myConnect;
        private bool _isOpen;
        
        /// <summary>
        /// Контроллер для непосредственного взаимодействия с базой данных
        /// </summary>
        public MySqlDataBaseController(ILogger logger, ICardioSettings settings)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _logger = logger;

            var initComand = "Database=" + settings.DataBaseSettings.DataBase + ";" +
                             "Data Source=" + settings.DataBaseSettings.Source +
                             ";User Id=" + settings.DataBaseSettings.User + 
                             ";Password=" + settings.DataBaseSettings.Password + 
                             ";Allow Zero Datetime=True;Convert Zero Datetime=True;charset=utf8";
            _myConnect =  new MySqlConnection(initComand);
            _isOpen = false;

        }

        /// <summary>
        /// Контроллер для непосредственного взаимодействия с базой данных
        /// </summary>
        public MySqlDataBaseController(
            ILogger logger,
            string dataBase, 
            string source, 
            string user, 
            string password)
        {
            _logger = logger;
            var initComand = "Database=" + dataBase + ";" +
                             "Data Source=" + source + 
                             ";User Id=" + user + 
                             ";Password=" + password + 
                             ";Allow Zero Datetime=True;Convert Zero Datetime=True;charset=utf8";
            _myConnect = new MySqlConnection(initComand);
            _isOpen = false;

        }

        /// <summary>
        /// Открывает соединение к базе данных, выполняя переданный запрос, возвращает MySqlDataReader для чтения результатов
        /// </summary>
        /// <param name="query">SQL запрос</param>
        /// <returns>MySqlDataReader для чтения результатов</returns>
        /// <remarks>Открывает соединение с базой данных. Пока его не закрыть, другие запросы выполняться не будут</remarks>
        /// <remarks>Использовать для запросов типа Select</remarks>
        public ISqlDataReader ConnectDb(string query)
        {
            if (_isOpen)
            {
                throw new AccessViolationException();
            }
            _isOpen = true;
            var command = new MySqlCommand(query, _myConnect);
            _myConnect.Open();
            var reader = command.ExecuteReader();
           
            return new CardioMySqlDataReader(reader);
        }

        /// <summary>
        /// Закрывает соединение с базой данных
        /// </summary>
        /// <param name="reader">MySqlDataReader, содержащий открытое соединение</param>
        public void DisсonnectDb(ISqlDataReader reader)
        {
            var mySqlReader = reader as CardioMySqlDataReader;
            if (mySqlReader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            _isOpen = false;
            mySqlReader.Reader?.Close();
            _myConnect.Close();
        }

        /// <summary>
        /// Выполняет запрос к базе данных
        /// </summary>
        /// <param name="query">Sql запрос</param>
        /// <remarks>Использовать для запросов типа Insert, Delete, Update, т.е. не возвращающих данных</remarks>
        public void ExecuteQuery(string query)
        {
            var comand = new MySqlCommand(query, _myConnect);
            _myConnect.Open();
            comand.ExecuteNonQuery();
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
                _logger.LogError("MySqlDataBaseController", e);
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
