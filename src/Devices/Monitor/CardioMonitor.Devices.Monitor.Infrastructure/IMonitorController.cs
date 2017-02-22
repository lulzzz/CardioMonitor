
using System.Threading.Tasks;
using CardioMonitor.BLL.CoreContracts.Session;

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
        /// <returns></returns>
        Task<bool> PumpCuffAsync();

        /// <summary>
        /// Возвращает показатели пациента, снятые с монитора
        /// </summary>
        /// <returns></returns>
        Task<PatientParams> GetPatientParamsAsync();
    }
}
