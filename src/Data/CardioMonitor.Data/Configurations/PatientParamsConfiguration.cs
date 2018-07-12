using System.Data.Entity.ModelConfiguration;
using CardioMonitor.Data.Ef.Entities.Sessions;

namespace CardioMonitor.Data.Ef.Configurations
{
    public class PatientParamsConfiguration : EntityTypeConfiguration<PatientParamsEntity>
    {
        public PatientParamsConfiguration()
        {
            ToTable("PatientParams").HasKey(x => x.Id);
            
            Property(x => x.AverageArterialPressure).HasColumnName("AverageArterialPressure");
            Property(x => x.AverageArterialPressureStatus).HasColumnName("AverageArterialPressureStatus");
            Property(x => x.DiastolicArterialPressure).HasColumnName("DiastolicArterialPressure");
            Property(x => x.DiastolicArterialPressureStatus).HasColumnName("DiastolicArterialPressureStatus");
            Property(x => x.HeartRate).HasColumnName("HeartRate");
            Property(x => x.HeartRateStatus).HasColumnName("HeartRateStatus");
            Property(x => x.InclinationAngle).HasColumnName("InclinationAngle");
            Property(x => x.Iteration).HasColumnName("Iteration");
            Property(x => x.RepsirationRate).HasColumnName("RepsirationRate");
            Property(x => x.RepsirationRateStatus).HasColumnName("RepsirationRateStatus");
            Property(x => x.Spo2).HasColumnName("Spo2");
            Property(x => x.Spo2Status).HasColumnName("Spo2Status");
            Property(x => x.SystolicArterialPressure).HasColumnName("SystolicArterialPressure");
            Property(x => x.SystolicArterialPressureStatus).HasColumnName("SystolicArterialPressureStatus");

        }
    }
}