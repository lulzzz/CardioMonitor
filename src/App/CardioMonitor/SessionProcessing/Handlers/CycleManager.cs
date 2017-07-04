using System.Threading.Tasks;
using CardioMonitor.SessionProcessing.Events;
using CardioMonitor.SessionProcessing.Events.Cycle;
using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Handlers
{
    /// <summary>
    /// Менеджер управления циклом
    /// </summary>
    public class CycleManager : 
        IEventHandler<CycleStartRequestedEvent>,
        IEventHandler<CycleCompletedEvent>
    {


        public Task Handle(CycleStartRequestedEvent command)
        {
            throw new System.NotImplementedException();
        }

        public Task Handle(CycleCompletedEvent command)
        {
            throw new System.NotImplementedException();
        }
    }
}