using System;
using System.Collections.Generic;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.Data.Contracts.UnitOfWork;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.CoreServices.Sessions
{
    public class SessionsService : ISessionsService
    {
        [NotNull]
        private readonly ICardioMonitorUnitOfWorkFactory _factory;

        public SessionsService(ICardioMonitorUnitOfWorkFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _factory = factory;
        }

        public void Add(Session patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            using (var uow = _factory.Create())
            {
                uow.Sessions.AddSession(patient.ToEntity());
            }
        }

        public List<Session> GetAll()
        {
            throw new System.NotImplementedException();
        }

        public void Edit(Session patient)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(Session patient)
        {
            throw new System.NotImplementedException();
        }
    }
}