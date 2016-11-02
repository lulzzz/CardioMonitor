using System.Data.Entity.ModelConfiguration;
using CardioMonitor.Data.Common.Entities.Sessions;

namespace CardioMonitor.Data.Ef.Configurations
{
    public class SessionConfiguration : EntityTypeConfiguration<Session>
    {
        public SessionConfiguration()
        {
            ToTable("Sessions").HasKey(x => x.Id);

            Property(x => x.DateTime).HasColumnName("DateTime");
            Property(x => x.Status).HasColumnName("Status");

            HasMany(x => x.Cycles).
                WithRequired(x => x.Session).
                HasForeignKey(x => x.Id);
        }
    }
}