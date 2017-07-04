using System.Threading.Tasks;
using CardioMonitor.SessionProcessing.Events;
using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Handlers
{
    /// <summary>
    /// Менеджер управления циклом
    /// </summary>
    public class CycleManager : 
        ICommandHandler<CycleStartedEvent>,
        ICommandHandler<CycleCompletedEvent>
    {
        public Task Handle(CycleStartedEvent command)
        {
            throw new System.NotImplementedException();
        }

        public Task Handle(CycleCompletedEvent command)
        {
            throw new System.NotImplementedException();
        }
    }
}