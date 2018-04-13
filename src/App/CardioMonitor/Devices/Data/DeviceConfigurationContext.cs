using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Reflection;
using CardioMonitor.Data.Ef.Context;

namespace CardioMonitor.Devices.Data
{
    internal class DeviceConfigurationContext : DbContext
    {
        public virtual DbSet<DeviceConfigurationEntity> DeviceConfigurations { get; set; }

        public DeviceConfigurationContext(string connection)
            : base(connection)
        {
            (this as IObjectContextAdapter).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<DeviceConfigurationContext>(null);
            modelBuilder.HasDefaultSchema("public");

            var typesToRegister = Assembly.GetAssembly(typeof(DeviceConfigurationContext))
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