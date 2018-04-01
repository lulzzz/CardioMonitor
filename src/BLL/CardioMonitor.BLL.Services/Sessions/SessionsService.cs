using System;
using System.Collections.Generic;
using System.Linq;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.CoreContracts.Session;
using CardioMonitor.BLL.Mappers;
using CardioMonitor.Data.Contracts.Entities.Sessions;
using CardioMonitor.Data.Contracts.UnitOfWork;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.CoreServices.Sessions
{
    public class SessionsService : ISessionsService
    {
        [NotNull]
        private readonly ICardioMonitorUnitOfWorkFactory _factory;

        private readonly IPatientsService _patientsService;

        public SessionsService([NotNull] ICardioMonitorUnitOfWorkFactory factory, [NotNull] IPatientsService patientsService)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _factory = factory;
            _patientsService = patientsService ?? throw new ArgumentNullException(nameof(patientsService));
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

        public List<SessionWithPatientInfo> GetSessions()
        {
            using (var uow = _factory.Create())
            {
                var sessions =  uow.Sessions.GetSessions()?.ToList() ?? new List<SessionEntity>(0);

                var patientIds = sessions.Select(x => x.Id);

                var patientNames = _patientsService.GetPatientNames(patientIds.ToList()) ??
                                   new List<PatientFullName>(0);

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

        
        public List<SessionInfo> GetPatientSessionInfos(int patientId)
        {
            using (var uow = _factory.Create())
            {
                return uow.Sessions.GetSessions(patientId)?.Select(x => x.ToInfoDomain()).ToList();
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