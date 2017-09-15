using CardioMonitor.SessionProcessing.Events.Session;
using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Events.Control
{
    /// <summary>
    /// Команда завершения процедуры (кровать вернулась в исходное положение в штатном режиме)
    /// </summary>
    /// <remarks>
    /// Событие может генерировать как извне, так изнутри подсистемы
    /// </remarks>
    internal class ProcedureCompletedEvent : IEvent
    {
    }
}