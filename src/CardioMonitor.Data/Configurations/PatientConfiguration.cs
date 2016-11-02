using System.Data.Entity.ModelConfiguration;
using CardioMonitor.Data.Common.Entities.Patients;

namespace CardioMonitor.Data.Ef.Configurations
{
    public class PatientConfiguration : EntityTypeConfiguration<Patient>
    {
        public PatientConfiguration()
        {
            ToTable("Patients").HasKey(x => x.Id);
            Property(x => x.FirstName).HasColumnName("FirstName");
            Property(x => x.PatronymicName).HasColumnName("PatronymicName");
            Property(x => x.LastName).HasColumnName("LastName");
            Property(x => x.BirthDate).HasColumnName("BirthDate");

            HasMany(x => x.Treatments)
                .WithRequired(t => t.Patient)
                .HasForeignKey(t => t.PatientId);
        }
    }
}