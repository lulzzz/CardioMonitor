using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Reflection;
using CardioMonitor.Data.Contracts.Entities.Patients;
using CardioMonitor.Data.Contracts.Entities.Sessions;
using CardioMonitor.Data.Contracts.Entities.Treatments;

namespace CardioMonitor.Data.Ef.Context
{
    public class CardioMonitorContext : DbContext
    {
        public virtual DbSet<PatientEntity> Patients { get; set; }
        public virtual DbSet<TreatmentEntity> Treatments { get; set; }
        public virtual DbSet<SessionEntity> Sessions { get; set; }

        public CardioMonitorContext()
        {
        }

        public CardioMonitorContext(string connectionString)
            : base(connectionString)
        {
            (this as IObjectContextAdapter).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var typesToRegister = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => !String.IsNullOrEmpty(type.Namespace))
                .Where(type => type.BaseType != null && type.BaseType.IsGenericType &&
                               type.BaseType.GetGenericTypeDefinition() ==
                               typeof(EntityTypeConfiguration<>));

            foreach (var type in typesToRegister)
            {
                dynamic configurationInstance = Activator.CreateInstance(type);
                modelBuilder.Configurations.Add(configurationInstance);
            }
            

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();


            return base.SaveChanges();
        }
    }
}