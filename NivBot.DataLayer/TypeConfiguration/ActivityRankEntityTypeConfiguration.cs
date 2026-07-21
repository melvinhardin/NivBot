using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using NivBot.DataLayer;
using NivBot.DataLayer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class SkillRankEntityTypeConfiguration : IEntityTypeConfiguration<SkillRank>
    {
        public void Configure(EntityTypeBuilder<SkillRank> builder)
        {
            builder
                .HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder
                .HasOne<DiscordRole>(x => x.DiscordRole)
                .WithOne()
                .HasForeignKey<SkillRank>(x => x.DiscordRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
