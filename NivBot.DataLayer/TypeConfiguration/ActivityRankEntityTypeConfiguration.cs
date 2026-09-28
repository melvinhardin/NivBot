using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using NivBot.DataLayer;
using NivBot.DataLayer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class ActivityRankEntityTypeConfiguration : IEntityTypeConfiguration<ActivityRank>
    {
        public void Configure(EntityTypeBuilder<ActivityRank> builder)
        {
            builder
                .HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
            builder
                .HasOne<Activity>(x => x.Activity)
                .WithMany()
                .HasForeignKey(x => x.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne<DiscordRole>(x => x.DiscordRole)
                .WithOne()
                .HasForeignKey<ActivityRank>(x => x.DiscordRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
