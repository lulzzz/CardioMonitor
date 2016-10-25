using System;
using System.Threading.Tasks;
using CardioMonitor.DataBase;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Repositories.Abstract;
using CardioMonitor.Resources;

namespace CardioMonitor.Repositories
{
    /// <summary>
    /// Репозиторий для доступа к базе данных
    /// </summary>
    internal class DataBaseRepository : IDataBaseRepository
    {
        private readonly DataBaseFactory _dataBaseFactory;
        private readonly ILogger _logger;

        public DataBaseRepository(
            DataBaseFactory dataBaseFactory,
            ILogger logger)
        {
            if (dataBaseFactory == null) throw new ArgumentNullException(nameof(dataBaseFactory));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _dataBaseFactory = dataBaseFactory;
            _logger = logger;
        }
        

        /// <summary>
        /// Возвращает статус соединения
        /// </summary>
        /// <returns></returns>
        public bool GetConnectionStatus()
        {
            var control = _dataBaseFactory.CreateDataBaseController();
            return control.GetConnectionStatus();
        }

        /// <summary>
        /// Проверяет соединение к базе
        /// </summary>
        /// <remarks>
        /// Может выбросить исключение с информацией о проблеме
        /// </remarks>
        public async Task CheckConnectionAsync(string dataBase, string source, string user, string password)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    var control = _dataBaseFactory.CreateDataBaseController(dataBase, source, user, password);
                    control.CheckConnection();
                }
                catch (Exception ex)
                {
                    _logger.LogError(nameof(DataBaseRepository), ex);
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
                            message = Localisation.DataBaseRepository_UnknownException;
                            break;
                    }
                    throw new Exception(message);
                }
            }).ConfigureAwait(false);

        }
        
    }
}
