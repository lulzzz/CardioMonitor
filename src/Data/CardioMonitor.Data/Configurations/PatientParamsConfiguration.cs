using System.Data.Entity.ModelConfiguration;
using CardioMonitor.Data.Contracts.Entities.Sessions;

namespace CardioMonitor.Data.Ef.Configurations
{
    public class PatientParamsConfiguration : EntityTypeConfiguration<PatientParamsEntity>
    {
        public PatientParamsConfiguration()
        {
            ToTable("PatientParams").HasKey(x => x.Id);
            
            Property(x => x.AverageArterialPressure).HasColumnName("AverageArterialPressure");
            Property(x => x.DiastolicArterialPressure).HasColumnName("DiastolicArterialPressure");
            Property(x => x.HeartRate).HasColumnName("HeartRate");
            Property(x => x.InclinationAngle).HasColumnName("InclinationAngle");
            Property(x => x.Iteration).HasColumnName("Iteration");
            Property(x => x.RepsirationRate).HasColumnName("RepsirationRate");
            Property(x => x.Spo2).HasColumnName("Spo2");
            Property(x => x.SystolicArterialPressure).HasColumnName("SystolicArterialPressure");

        }
    }
}