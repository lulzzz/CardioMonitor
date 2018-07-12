using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.CoreContracts.Session.Events;
using CardioMonitor.BLL.Mappers;
using CardioMonitor.Data.Ef.Context;
using JetBrains.Annotations;
using Markeli.Utils.EventBus.Contracts;
using ThenInclude.EF6;

namespace CardioMonitor.BLL.CoreServices.Sessions
{
    public class SessionsService : ISessionsService
    {
        [NotNull]
        private readonly ICardioMonitorContextFactory _factory;

        [NotNull]
        private readonly IEventBus _eventBus;

        public SessionsService(
            [NotNull] ICardioMonitorContextFactory factory, [NotNull] IEventBus eventBus)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _eventBus = eventBus;
        }

        public async Task<int> AddAsync(Session session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            using (var context = _factory.Create())
            {
                var entity = session.ToEntity();
                context.Sessions.Add(entity);
                await context
                    .SaveChangesAsync()
                    .ConfigureAwait(false);

                await _eventBus
                    .PublishAsync(new SessionAddedEvent(entity.Id))
                    .ConfigureAwait(false);

                return entity.Id;
            }
        }

        public async Task EditAsync(Session session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            using (var context = _factory.Create())
            {
                if (!context.Sessions.Any(x => x.Id == session.Id)) 
                    throw new ArgumentException(nameof(session.Id));
                
                var entity = session.ToEntity();
                context.Sessions.Attach(entity);
                foreach (var cycleEntity in entity.Cycles)
                {
                    context.SessionCycles.Attach(cycleEntity);
                    context.Entry(entity).State = EntityState.Modified;
                    foreach (var paramsEntity in cycleEntity.PatientParams)
                    {
                        context.PatientParams.Attach(paramsEntity);
                        context.Entry(paramsEntity).State = EntityState.Modified;
                    }
                }
                context.Entry(entity).State = EntityState.Modified;
                
                await context
                    .SaveChangesAsync()
                    .ConfigureAwait(false);

                await _eventBus
                    .PublishAsync(new SessionChangedEvent(entity.Id))
                    .ConfigureAwait(false);
            }
        }


        public async Task<Session> GetAsync(int sessionId)
        {
            using (var context = _factory.Create())
            {
                var result = await context.Sessions
                    .Include(x => x.Cycles.Select(y => y.PatientParams))
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == sessionId)
                    .ConfigureAwait(false);
                return result?.ToDomain();
            }
        }

        public async Task<ICollection<SessionWithPatientInfo>> GetAllAsync()
        {
            using (var context = _factory.Create())
            {
                var sessions = await context.Sessions
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);
                if (sessions.Count == 0) return new List<SessionWithPatientInfo>(0);

                var patientIds = new HashSet<int>(sessions.Select(x => x.PatientId));

                var patients = await context
                    .Patients
                    .AsNoTracking()
                    .Where(x => patientIds.Contains(x.Id))
                    .ToListAsync()
                    .ConfigureAwait(false);

                var patientNames = patients.Select(x => new PatientFullName
                {
                    PatientId = x.Id,
                    LastName = x.LastName,
                    FirstName = x.FirstName,
                    PatronymicName = x.PatronymicName
                });

                var patientNamesMap = new Dictionary<int, PatientFullName>();
                foreach (var patientFullName in patientNames)
                {
                    patientNamesMap[patientFullName.PatientId] = patientFullName;
                }

                var result = new List<SessionWithPatientInfo>(sessions.Count);
                foreach (var sessionInfo in sessions)
                {
                    if (!patientNamesMap.ContainsKey(sessionInfo.PatientId)) continue;

                    result.Add(new SessionWithPatientInfo
                    {
                        Id = sessionInfo.Id,
                        Status = sessionInfo.Status.ToSessionStatus(),
                        TimestampUtc = sessionInfo.TimestampUtc,
                        PatientId = sessionInfo.PatientId,
                        PatientFullName = patientNamesMap[sessionInfo.PatientId].Name
                    });
                }

                return result;
            }
        }

        
        public async Task<ICollection<SessionInfo>> GetPatientSessionInfosAsync(int patientId)
        {
            using (var context = _factory.Create())
            {
                var result = await context
                    .Sessions
                    .AsNoTracking()
                    .Where(x => x.PatientId == patientId)
                    .ToListAsync()
                    .ConfigureAwait(false);
                return result.Select(x => x.ToInfoDomain()).ToList();
            }
        }


        public async Task DeleteAsync(int sessionId)
        {
            using (var context = _factory.Create())
            {
                var session = await context
                    .Sessions
                    .FirstOrDefaultAsync(x => x.Id == sessionId)
                    .ConfigureAwait(false);
                if (session == null) throw new ArgumentException();

                context.Sessions.Remove(session);
                await context
                    .SaveChangesAsync()
                    .ConfigureAwait(false);

                await _eventBus
                    .PublishAsync(new SessionDeletedEvent(sessionId))
                    .ConfigureAwait(false);
            }
        }
    }
}