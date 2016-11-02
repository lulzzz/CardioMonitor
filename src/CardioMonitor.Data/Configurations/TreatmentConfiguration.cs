using System.Data.Entity.ModelConfiguration;
using CardioMonitor.Data.Common.Entities.Treatments;

namespace CardioMonitor.Data.Ef.Configurations
{
    public class TreatmentConfiguration :EntityTypeConfiguration<Treatment>
    {
        public TreatmentConfiguration()
        {
            ToTable("Treatments").HasKey(x => x.Id);
            Property(x => x.LastSessionDate).HasColumnName("LastSessionDate");
            Property(x => x.StartDate).HasColumnName("StartDate");
            
            HasMany(x => x.Sessions)
                .WithRequired(t => t.Treatment)
                .HasForeignKey(t => t.TreatmentId);
        }
    }
}