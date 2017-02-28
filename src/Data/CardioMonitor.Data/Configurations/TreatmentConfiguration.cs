using System.Data.Entity.ModelConfiguration;
using CardioMonitor.Data.Contracts.Entities.Treatments;

namespace CardioMonitor.Data.Ef.Configurations
{
    public class TreatmentConfiguration :EntityTypeConfiguration<TreatmentEntity>
    {
        public TreatmentConfiguration()
        {
            ToTable("Treatments").HasKey(x => x.Id);
            Property(x => x.StartDate).HasColumnName("StartDate");
            
            HasMany(x => x.Sessions)
                .WithRequired(t => t.TreatmentEntity)
                .HasForeignKey(t => t.TreatmentId);
        }
    }
}