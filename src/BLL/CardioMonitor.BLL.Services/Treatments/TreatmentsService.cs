using System;
using System.Collections.Generic;
using System.Linq;
using CardioMonitor.BLL.CoreContracts.Treatment;
using CardioMonitor.BLL.Mappers;
using CardioMonitor.Data.Contracts.UnitOfWork;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.CoreServices.Treatments
{
    public class TreatmentsService : ITreatmentsService
    {
        [NotNull]
        private readonly ICardioMonitorUnitOfWorkFactory _factory;

        public TreatmentsService([NotNull] ICardioMonitorUnitOfWorkFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            _factory = factory;
        }

        public void Add([NotNull] Treatment patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            using (var uow = _factory.Create())
            {
                uow.BeginTransation();
                uow.Treatments.AddTreatment(patient.ToEntity());
                uow.Commit();
            }
        }

        public List<Treatment> GetAll(int patientId)
        {
            using (var uow = _factory.Create())
            {
                return uow.Treatments.GetTreatments(patientId).Select(x => x.ToDomain()).ToList();
            }
        }
    }
}