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
        /// <summary>
        /// Накачка манжеты рукава для измерения давления
        /// </summary>
        Task PumpCuffAsync();

        /// <summary>
        /// Возвращает показатели пациента, снятые с монитора
        /// </summary>
        /// <returns></returns>
        [NotNull]
        Task<PatientCommonParams> GetPatientParamsAsync();

        /// <summary>
        /// Возвращает параметры пациента, связанные с давлением
        /// </summary>
        /// <returns></returns>
        [NotNull]
        Task<PatientPressureParams> GetPatientPressureParamsAsync();

        /// <summary>
        /// Возвращает ЭКГ
        /// </summary>
        /// <param name="duration">Длительность снятия ЭКГ (чтобы потом было легче изменять)</param>
        /// <returns></returns>
        Task<PatientEcgParams> GetPatientEcgParamsAsync(TimeSpan duration);
    }
}
