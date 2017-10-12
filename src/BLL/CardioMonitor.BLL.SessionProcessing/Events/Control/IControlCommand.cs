using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Events.Control
{
    /// <summary>
    /// Команды управления сеансом
    /// </summary> 
    /// <remarks>
    /// Может быть передана как с UI, так и c железа
    /// </remarks>
    public interface IControlCommand : IEvent
    {
        /// <summary>
        /// Была ли команда дана из приложения (т.е. из UI)
        /// </summary>
        bool IsInternal { get; set; }

        //todo exception
    }
}