using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class DiscordRoleEntityTypeConfiguration : IEntityTypeConfiguration<DiscordRole>
    {
        public void Configure(EntityTypeBuilder<DiscordRole> builder)
        {
            builder
                .HasKey(x => x.DiscordRoleId);
            builder
                .Property(x => x.DiscordRoleId)
                .ValueGeneratedNever();

            builder
                .HasIndex(x => x.GuildId);

            builder
                .HasOne(x => x.Activity)
                .WithMany()
                .HasForeignKey(x => x.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(x => x.Item)
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne<Skill>()
                .WithMany()
                .HasForeignKey(x => x.Skill)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .ToTable(t =>
                {
                    t.HasCheckConstraint("CK_DiscordRole_Name", "length(\"Name\") <= 100");
                    t.HasCheckConstraint("CK_DiscordRole_Threshold", "\"Threshold\" >= 0");
                    t.HasCheckConstraint("CK_DiscordRole_TierGroup", "length(\"TierGroup\") <= 50");
                    t.HasCheckConstraint("CK_DiscordRole_Emoji", "length(\"Emoji\") <= 100");
                });
        }
    }
}
