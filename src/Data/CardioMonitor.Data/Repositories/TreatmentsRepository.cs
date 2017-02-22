using System;
using System.Collections.Generic;
using System.Linq;
using CardioMonitor.Data.Contracts.Entities.Treatments;
using CardioMonitor.Data.Contracts.Repositories;
using CardioMonitor.Data.Ef.Context;
using JetBrains.Annotations;

namespace CardioMonitor.Data.Ef.Repositories
{
    internal class TreatmentsRepository : ITreatmentsRepository
    {
        [NotNull]
        private readonly CardioMonitorContext _context;

        public TreatmentsRepository(CardioMonitorContext context)
        {
            _context = context;
        }

        public List<TreatmentEntity> GetTreatments(int patientId)
        {
            return new List<TreatmentEntity>(
                from t in _context.Treatments
                    where t.PatientId == patientId
                    select t);
        }

        public void AddTreatment([CanBeNull] TreatmentEntity treatmentEntity)
        {
            if (treatmentEntity == null) throw new ArgumentNullException(nameof(treatmentEntity));

            _context.Treatments.Add(treatmentEntity);
        }

        public void DeleteTreatment(int treatmentId)
        {
            var treatment = (from t in _context.Treatments
                where t.Id == treatmentId
                select t).FirstOrDefault();
            if (treatment == null) return;

            _context.Treatments.Remove(treatment);
        }
    }
}