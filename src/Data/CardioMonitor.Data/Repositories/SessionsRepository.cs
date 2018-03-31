using System;
using System.Collections.Generic;
using System.Linq;
using CardioMonitor.Data.Contracts.Entities.Sessions;
using CardioMonitor.Data.Contracts.Repositories;
using CardioMonitor.Data.Ef.Context;
using JetBrains.Annotations;

namespace CardioMonitor.Data.Ef.Repositories
{
    internal class SessionsRepository : ISessionsRepository
    {
        [NotNull]
        private readonly CardioMonitorContext _context;

        public SessionsRepository(CardioMonitorContext context)
        {
            _context = context;
        }

        public void AddSession(SessionEntity sessionEntity)
        {
            if (sessionEntity == null) throw new ArgumentNullException(nameof(sessionEntity));

            _context.Sessions.Add(sessionEntity);
        }

        public SessionEntity GetSession(int sessionId)
        {
            return (from session in _context.Sessions
                where session.Id == sessionId
                select session).FirstOrDefault();
        }

        public List<SessionEntity> GetSessions(int patientId)
        {
            return new List<SessionEntity>(_context.Sessions.Where(x => x.PatientId == patientId));
        }

        public void DeleteSession(int sessionId)
        {
            var result = (from session in _context.Sessions
                where session.Id == sessionId
                select session).FirstOrDefault();

            if (result == null) return;

            _context.Sessions.Remove(result);
        }

    }
}