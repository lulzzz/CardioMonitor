using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Events.Devices
{
    /// <summary>
    /// Запрос на получение данных пациента для монитора
    /// </summary>
    internal class PatientParamsRequestEvent : IEvent
    {
        /// <summary>
        /// Текущий угол наклона кровати
        /// </summary>
        public double CurrentAngle { get; set; }
    }
}