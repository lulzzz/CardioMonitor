using System;
using System.Collections.Generic;
using System.Linq;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.Mappers;
using CardioMonitor.Data.Contracts.UnitOfWork;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.CoreServices.Sessions
{
    public class SessionsService : ISessionsService
    {
        [NotNull]
        private readonly ICardioMonitorUnitOfWorkFactory _factory;

        public SessionsService([NotNull] ICardioMonitorUnitOfWorkFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _factory = factory;
        }

        public void Add(Session session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            using (var uow = _factory.Create())
            {
                uow.BeginTransation();

                uow.Sessions.AddSession(session.ToEntity());
                uow.Commit();
            }
        }


        public Session Get(int sessionId)
        {
            using (var uow = _factory.Create())
            {
                return uow.Sessions.GetSession(sessionId).ToDomain();
            }
        }


        public List<Session> GetAll(int treatmentId)
        {
            using (var uow = _factory.Create())
            {
                return uow.Sessions.GetSessions(treatmentId)?.Select(x => x.ToDomain()).ToList();
            }
        }


        public List<SessionInfo> GetInfos(int treatmentId)
        {
            using (var uow = _factory.Create())
            {
                return uow.Sessions.GetSessions(treatmentId)?.Select(x => x.ToInfoDomain()).ToList();
            }
        }


        public void Delete(int sessionId)
        {
            using (var uow = _factory.Create())
            {
                uow.BeginTransation();
                uow.Sessions.DeleteSession(sessionId);
                uow.Commit();
            }
        }
    }
}