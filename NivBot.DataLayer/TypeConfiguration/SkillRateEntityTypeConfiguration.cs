using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using NivBot.DataLayer.Seed;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class SkillRateEntityTypeConfiguration : IEntityTypeConfiguration<SkillRate>
    {
        public void Configure(EntityTypeBuilder<SkillRate> builder)
        {
            builder
                .HasKey(x => new { x.Skill, x.StartXp });

            builder
                .HasOne<Skill>()
                .WithMany()
                .HasForeignKey(x => x.Skill)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .ToTable(t =>
                {
                    t.HasCheckConstraint("CK_SkillRate_StartXp", "\"StartXp\" >= 0 AND \"StartXp\" <= 200000000");
                    t.HasCheckConstraint("CK_SkillRate_XpPerHour", "\"XpPerHour\" >= 0");
                });

            builder.HasData(WiseOldManRates.SkillRates);
        }
    }
}
