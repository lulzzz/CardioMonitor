﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.Mappers;
using CardioMonitor.Data.Ef.Context;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.CoreServices.Sessions
{
    public class SessionsService : ISessionsService
    {
        [NotNull]
        private readonly ICardioMonitorContextFactory _factory;

        public SessionsService(
            [NotNull] ICardioMonitorContextFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async Task AddAsync(Session session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            using (var context = _factory.Create())
            {

                context.Sessions.Add(session.ToEntity());
                await context
                    .SaveChangesAsync()
                    .ConfigureAwait(false);
            }
        }


        public async Task<Session> GetAsync(int sessionId)
        {
            using (var context = _factory.Create())
            {
                var result = await context.Sessions
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

                var patientIds = new HashSet<int>(sessions.Select(x => x.Id));

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
                        DateTime = sessionInfo.DateTime,
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
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}