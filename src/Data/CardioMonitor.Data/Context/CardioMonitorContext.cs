﻿using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Reflection;
using CardioMonitor.Data.Ef.Entities.Patients;
using CardioMonitor.Data.Ef.Entities.Sessions;

namespace CardioMonitor.Data.Ef.Context
{
    public class CardioMonitorContext : DbContext
    {
        public virtual DbSet<PatientEntity> Patients { get; set; }
        public virtual DbSet<SessionEntity> Sessions { get; set; }
        public virtual DbSet<SessionCycleEntity> SessionCycles { get; set; }
        public virtual DbSet<PatientParamsEntity> PatientParams { get; set; }

        static CardioMonitorContext()
        {
            //Database.SetInitializer(new ContextInitializer());
        }

        public CardioMonitorContext(string connection)
            : base(connection)
        {
            (this as IObjectContextAdapter).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<CardioMonitorContext>(null);
            modelBuilder.HasDefaultSchema("public");

            var typesToRegister = Assembly.GetAssembly(typeof(CardioMonitorContext))
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