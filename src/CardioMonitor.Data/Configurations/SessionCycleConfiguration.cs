using System.Data.Entity.ModelConfiguration;
using CardioMonitor.Data.Common.Entities.Sessions;

namespace CardioMonitor.Data.Ef.Configurations
{
    public class SessionCycleConfiguration : EntityTypeConfiguration<SessionCycle>
    {
        public SessionCycleConfiguration()
        {
            ToTable("SessionCycles").HasKey(x => x.Id);
            
            Property(x => x.CycleNumber).HasColumnName("CycleNumber");

            HasMany(x => x.PatientParams).
                WithRequired(x => x.SessionCycle).
                HasForeignKey(x => x.SessionCycleId);
        }
    }
}