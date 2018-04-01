using System;
using System.Collections.Generic;
using System.Linq;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.Mappers;
using CardioMonitor.Data.Contracts.UnitOfWork;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.CoreServices.Patients
{
    public class PatientService : IPatientsService
    {
        [NotNull]
        private readonly ICardioMonitorUnitOfWorkFactory _factory;

        public PatientService(ICardioMonitorUnitOfWorkFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public void Add([NotNull] Patient patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            using (var uow = _factory.Create())
            {
                uow.BeginTransation();
                uow.Patients.AddPatient(patient.ToEntity());
                uow.Commit();
            }
        }

        public List<PatientFullName> GetPatientNames(ICollection<int> patientIds)
        {
            using (var uow = _factory.Create())
            {
                var result = uow.Patients.GetPatients(patientIds).Select(x => new PatientFullName
                {
                    PatientId = x.Id,
                    LastName = x.LastName,
                    FirstName = x.FirstName,
                    PatronymicName = x.PatronymicName
                });
                return new List<PatientFullName>(result);
            }
        }

        public Patient GetPatient(int patientId)
        {
            using (var uow = _factory.Create())
            {
                return uow.Patients.GetPatient(patientId)?.ToDomain();
            }
        }

        public List<Patient> GetAll()
        {
            using (var uow = _factory.Create())
            {
                var result = uow.Patients.GetPatients().Select(x => x.ToDomain());
                return  new List<Patient>(result);
            }
        }

        public void Edit(Patient patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            using (var uow = _factory.Create())
            {
                uow.BeginTransation();
                uow.Patients.UpdatePatient(patient.ToEntity());
                uow.Commit();
            }
        }

        public void Delete(int patientId)
        {
            using (var uow = _factory.Create())
            {
                uow.BeginTransation();
                uow.Patients.DeletePatient(patientId);
                uow.Commit();
            }
        }
    }
}