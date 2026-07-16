using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class GoodplaceActivityTaskEntityTypeConfiguration : IEntityTypeConfiguration<GoodplaceActivityTask>
    {
        public void Configure(EntityTypeBuilder<GoodplaceActivityTask> builder)
        {
            builder
                .HasKey(x => x.GoodplaceUserId);

            builder
                .HasOne<Activity>(x => x.Activity)
                .WithMany()
                .HasForeignKey(x => x.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne<GoodplaceUser>(x => x.GoodplaceUser)
                .WithOne(x => x.GoodplaceActivityTask)
                .HasForeignKey<GoodplaceUser>(x => x.DiscordUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
