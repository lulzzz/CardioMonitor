using System;
using System.Threading.Tasks;
using CardioMonitor.SessionProcessing.Events;
using CardioMonitor.SessionProcessing.Events.Session;
using Enexure.MicroBus;

namespace CardioMonitor.SessionProcessing.Handlers
{
    /// <summary>
    /// Управляет всем процессом сеанса
    /// </summary>
    public class SessionManager :
        IEventHandler<SessionStartEvent>,
        IEventHandler<SessionEmergencyStoppedEvent>,
        IEventHandler<SessionResumedEvent>,
        IEventHandler<SessionSuspendedEvent>,
        IEventHandler<SessionCompletedEvent>
    {
        public Task Handle(SessionStartEvent command)
        {
            throw new NotImplementedException();
        }

        public Task Handle(SessionCompletedEvent command)
        {
            throw new NotImplementedException();
        }

        public Task Handle(SessionSuspendedEvent command)
        {
            throw new NotImplementedException();
        }

        public Task Handle(SessionEmergencyStoppedEvent command)
        {
            throw new NotImplementedException();
        }

        public Task Handle(SessionResumedEvent command)
        {
            throw new NotImplementedException();
        }
    }
}