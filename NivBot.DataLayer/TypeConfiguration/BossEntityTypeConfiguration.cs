using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using NivBot.DataLayer.Seed;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class BossEntityTypeConfiguration : IEntityTypeConfiguration<Boss>
    {
        public void Configure(EntityTypeBuilder<Boss> builder)
        {
            builder
                .HasKey(x => x.OsrsName);

            builder
                .ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Boss_OsrsName", "length(\"OsrsName\") <= 100");
                    t.HasCheckConstraint("CK_Boss_KillsPerHour", "\"KillsPerHour\" >= 0");
                    t.HasCheckConstraint("CK_Boss_MinimumKc", "\"MinimumKc\" >= 0");
                });

            builder.HasData(WiseOldManRates.Bosses);
        }
    }
}
