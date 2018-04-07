using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CardioMonitor.BLL.CoreContracts.Patients;
using CardioMonitor.BLL.Mappers;
using CardioMonitor.Data.Ef.Context;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.CoreServices.Patients
{
    public class PatientService : IPatientsService
    {
        [NotNull]
        private readonly ICardioMonitorContextFactory _contextFactory;

        public PatientService(ICardioMonitorContextFactory contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task AddAsync([NotNull] Patient patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            using (var context = _contextFactory.Create())
            {
                context.Patients.Add(patient.ToEntity());
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<ICollection<PatientFullName>> GetPatientNamesAsync([NotNull] ICollection<int> patientIds)
        {
            if (patientIds == null) throw new ArgumentNullException(nameof(patientIds));
            using (var context = _contextFactory.Create())
            {
                var uniquePatientIds = new HashSet<int>(patientIds);
                var result = await context.Patients
                    .AsNoTracking()
                    .Where(x => uniquePatientIds.Contains(x.Id))
                    .ToListAsync()
                    .ConfigureAwait(false);

               return new List<PatientFullName>(result.Select(x => new PatientFullName
                {
                    PatientId = x.Id,
                    LastName = x.LastName,
                    FirstName = x.FirstName,
                    PatronymicName = x.PatronymicName
                }));
            }
        }

        public async Task<ICollection<PatientFullName>> GetPatientNamesAsync()
        {
            using (var context = _contextFactory.Create())
            {
                var result = await context.Patients
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);

                return new List<PatientFullName>(result.Select(x => new PatientFullName
                {
                    PatientId = x.Id,
                    LastName = x.LastName,
                    FirstName = x.FirstName,
                    PatronymicName = x.PatronymicName
                }));
            }
        }

        public async Task<Patient> GetPatientAsync(int patientId)
        {
            using (var uow = _contextFactory.Create())
            {
                var result = await uow.Patients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == patientId)
                    .ConfigureAwait(false);

                return result?.ToDomain();
            }
        }

        public async Task<ICollection<Patient>> GetAllAsync()
        {
            using (var context = _contextFactory.Create())
            {
                var result = await context.Patients
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);

                return  new List<Patient>(result.Select(x => x.ToDomain()));
            }
        }

        public async Task EditAsync(Patient patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            using (var context = _contextFactory.Create())
            {
                var entity = patient.ToEntity();
                context.Patients.Attach(entity);
                context.Entry(entity).State = EntityState.Modified;
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task DeleteAsync(int patientId)
        {
            using (var context = _contextFactory.Create())
            {
                var patient = await context.Patients
                    .FirstOrDefaultAsync(x => x.Id == patientId)
                    .ConfigureAwait(false);
                if (patient == null) throw new ArgumentException();

                context.Patients.Remove(patient);
                await context
                    .SaveChangesAsync()
                    .ConfigureAwait(false);
            }
        }
    }
}