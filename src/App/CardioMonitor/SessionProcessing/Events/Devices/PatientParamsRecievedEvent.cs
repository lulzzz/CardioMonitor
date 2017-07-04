using CardioMonitor.BLL.CoreContracts.Session;
using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Events.Devices
{
    /// <summary>
    /// Событие получения данные пациента
    /// </summary>
    public class PatientParamsRecievedEvent : IEvent
    {
        /// <summary>
        /// Показатели пациента
        /// </summary>
        public PatientParams Params { get; set; }
    }
}