using System;
using System.Collections.Generic;
using System.Linq;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.Mappers;
using CardioMonitor.Data.Contracts.UnitOfWork;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.CoreServices.Patients
{
    internal class PatientService : IPatientsService
    {
        [NotNull]
        private readonly ICardioMonitorUnitOfWorkFactory _factory;

        public PatientService(ICardioMonitorUnitOfWorkFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _factory = factory;
        }

        public void Add([NotNull] Patient patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            using (var uow = _factory.Create())
            {
                uow.Patients.AddPatient(patient.ToEntity());
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
                uow.Patients.UpdatePatient(patient.ToEntity());
            }
        }

        public void Delete(Patient patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            using (var uow = _factory.Create())
            {
                uow.Patients.DeletePatient(patient.Id);
            }
        }
    }
}