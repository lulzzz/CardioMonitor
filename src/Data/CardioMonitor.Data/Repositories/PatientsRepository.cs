using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CardioMonitor.Data.Contracts.Entities.Patients;
using CardioMonitor.Data.Contracts.Repositories;
using CardioMonitor.Data.Ef.Context;
using JetBrains.Annotations;

namespace CardioMonitor.Data.Ef.Repositories
{
    internal class PatientsRepository : IPatientsRepository
    {
        [NotNull]
        private readonly CardioMonitorContext _context;

        public PatientsRepository(CardioMonitorContext context)
        {
            _context = context;
        }

        public List<PatientEntity> GetPatients()
        {
            return _context.Patients.ToList();
        }

        public void AddPatient([NotNull] PatientEntity patientEntity)
        {
            if (patientEntity == null) throw new ArgumentNullException(nameof(patientEntity));

            _context.Patients.Add(patientEntity);
        }

        public void UpdatePatient([NotNull] PatientEntity patientEntity)
        {
            if (patientEntity == null) throw new ArgumentNullException(nameof(patientEntity));

            _context.Patients.Attach(patientEntity);

            _context.Entry(patientEntity).State = EntityState.Modified;
        }

        public void DeletePatient(int patientId)
        {
            var patient = (from p in _context.Patients
                where p.Id == patientId
                select p).FirstOrDefault();
            if (patient == null) return;

            _context.Patients.Remove(patient);
        }
    }
}