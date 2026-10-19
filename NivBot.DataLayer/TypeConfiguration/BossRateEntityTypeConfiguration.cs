using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using NivBot.DataLayer.Seed;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class BossRateEntityTypeConfiguration : IEntityTypeConfiguration<BossRate>
    {
        public void Configure(EntityTypeBuilder<BossRate> builder)
        {
            builder
                .HasKey(x => x.OsrsName);

            builder
                .ToTable(t =>
                {
                    t.HasCheckConstraint("CK_BossRate_OsrsName", "length(\"OsrsName\") <= 100");
                    t.HasCheckConstraint("CK_BossRate_KillsPerHour", "\"KillsPerHour\" >= 0");
                    t.HasCheckConstraint("CK_BossRate_MinimumKc", "\"MinimumKc\" >= 0");
                });

            builder.HasData(WiseOldManRates.BossRates);
        }
    }
}
