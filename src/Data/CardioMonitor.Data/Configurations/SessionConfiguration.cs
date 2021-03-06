﻿using System.Data.Entity.ModelConfiguration;
using CardioMonitor.Data.Ef.Entities.Sessions;

namespace CardioMonitor.Data.Ef.Configurations
{
    public class SessionConfiguration : EntityTypeConfiguration<SessionEntity>
    {
        public SessionConfiguration()
        {
            ToTable("Sessions").HasKey(x => x.Id);

            Property(x => x.TimestampUtc).HasColumnName("TimestampUtc");
            Property(x => x.Status).HasColumnName("Status");

            HasMany(x => x.Cycles).
                WithRequired(x => x.SessionEntity).
                HasForeignKey(x => x.SessionId);
        }
    }
}