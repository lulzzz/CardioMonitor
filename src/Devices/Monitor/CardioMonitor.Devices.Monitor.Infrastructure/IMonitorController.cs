using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CardioMonitor.Devices.Monitor.Infrastructure
{
    /// <summary>
    /// Контроллер для взаимодействия с медицинским монитором
    /// </summary>
    public interface IMonitorController : IDeviceController
    {
        #region Управление контроллером
        
        /// <summary>
        /// Флаг наличия соедения с устройством
        /// </summary>
        bool IsConnected { get; }
        
        /// <summary>
        /// Выполняет инициализацию контроллера
        /// </summary>
        /// <param name="config"></param>
        void Init([NotNull] IMonitorControllerConfig config);

        /// <summary>
        /// Асинхронно подключается к устройству
        /// </summary>
        /// <returns></returns>
        Task ConnectAsync();

        /// <summary>
        /// Асинхронно отключается от устройства
        /// </summary>
        /// <returns></returns>
        Task DisconnectAsync();
        
        /// <summary>
        /// Отправляет команду накачки манжеты рукава для измерения давления
        /// </summary>
        Task PumpCuffAsync();
      
        #endregion

        #region Получение данных

        /// <summary>
        /// Возвращает общие показатели пациента, снятые с монитора
        /// </summary>
        /// <returns></returns>
        [NotNull]
        Task<PatientCommonParams> GetPatientCommonParamsAsync();

        /// <summary>
        /// Возвращает параметры давлением пациента, снятые с монитора
        /// </summary>
        /// <returns></returns>
        [NotNull]
        Task<PatientPressureParams> GetPatientPressureParamsAsync();

        /// <summary>
        /// Возвращает ЭКГ
        /// </summary>
        /// <param name="duration">Длительность снятия ЭКГ (чтобы потом было легче изменять)</param>
        /// <returns></returns>
        [NotNull]
        Task<PatientEcgParams> GetPatientEcgParamsAsync(TimeSpan duration);

        #endregion
    }
}
