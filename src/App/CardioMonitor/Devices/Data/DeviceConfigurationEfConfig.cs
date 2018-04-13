using System.Data.Entity.ModelConfiguration;

namespace CardioMonitor.Devices.Data
{
    internal class DeviceConfigurationEfConfig : EntityTypeConfiguration<DeviceConfigurationEntity>
    {
        public DeviceConfigurationEfConfig()
        {
            ToTable("DeviceConfigirations").HasKey(x => x.ConfigId);

            Property(x => x.ConfigName).HasColumnName("ConfigName");
            Property(x => x.DeviceId).HasColumnName("DeviceId");
            Property(x => x.DeviceTypeId).HasColumnName("DeviceTypeId");
            Property(x => x.ParamsJson).HasColumnName("ParamsJson");
        }
    }
}