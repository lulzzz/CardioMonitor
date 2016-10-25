using System;
using CardioMonitor.DataBase.MySql;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Settings;

namespace CardioMonitor.DataBase
{
    /// <summary>
    /// Фабрика по созданию всех объектов для взаимодействия с базой
    /// </summary>
    public class DataBaseFactory
    {
        /// <summary>
        /// Логгер
        /// </summary>
        private readonly ILogger _logger;

        private readonly ICardioSettings _settings;

        /// <summary>
        /// Фабрика по созданию всех объектов для взаимодействия с базой
        /// </summary>
        /// <param name="logger">Логгер</param>
        /// <param name="settings"></param>
        public DataBaseFactory(
            ILogger logger,
            ICardioSettings settings)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        /// Создает контроллер, для взаимодействия с базой
        /// </summary>
        /// <returns></returns>
        public IDataBaseController CreateDataBaseController()
        {
            return new MySqlDataBaseController(_logger, _settings);
        }

        /// <summary>
        /// Создает контроллер, для взаимодействия с базой 
        /// </summary>
        /// <param name="dataBase">Название базы</param>
        /// <param name="source">Адрес</param>
        /// <param name="user">Пользователь</param>
        /// <param name="password">Пароль</param>
        /// <returns></returns>
        public IDataBaseController CreateDataBaseController(string dataBase, string source, string user, string password)
        {
            return new MySqlDataBaseController(_logger, dataBase, source, user, password);
        }

        /// <summary>
        /// Создает объект для безопасного чтения данных из базы
        /// </summary>
        /// <param name="reader">Объект для чтения данных из базы</param>
        /// <returns>Объект для безопасного чтения данных из базы</returns>
        public ISafeReader CreateSafeReader(ISqlDataReader reader)
        {
            var typedReader = reader as CardioMySqlDataReader;
            if (typedReader != null)
            {
                return new MySqlSafeReader(typedReader.Reader);
            }

            return null;
        }

        /// <summary>
        /// Создает переводчик ошибок, который по исключению и коду ошибку возвращает понятную для приложения информацию
        /// </summary>
        /// <returns></returns>
        public IDataBaseErrorTranslator CreateDataBaseErrorTranslator()
        {
            return new MySqlErrorTranslator();
        }
    }
}