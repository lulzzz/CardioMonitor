
using CardioMonitor.Models.Session;

namespace CardioMonitor.Devices.Monitor.Infrastructure
{
    /// <summary>
    /// Контроллер для взаимодействия с медицинским монитором
    /// </summary>
    interface IMonitorController
    {
        /// <summary>
        /// Накачка манжеты рукава для измерения давления
        /// </summary>
        /// <returns></returns>
        bool PumpCuff();

        /// <summary>
        /// Возвращает показатели пациента, снятые с монитора
        /// </summary>
        /// <returns></returns>
        PatientParams GetPatientParams();
    }
}
