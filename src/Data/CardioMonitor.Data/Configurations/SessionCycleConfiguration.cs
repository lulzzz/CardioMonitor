using System.Data.Entity.ModelConfiguration;
using CardioMonitor.Data.Contracts.Entities.Sessions;

namespace CardioMonitor.Data.Ef.Configurations
{
    public class SessionCycleConfiguration : EntityTypeConfiguration<SessionCycleEntity>
    {
        public SessionCycleConfiguration()
        {
            ToTable("SessionCycles").HasKey(x => x.Id);
            
            Property(x => x.CycleNumber).HasColumnName("CycleNumber");

            HasMany(x => x.PatientParams).
                WithRequired(x => x.SessionCycleEntity).
                HasForeignKey(x => x.SessionCycleId);
        }
    }
}