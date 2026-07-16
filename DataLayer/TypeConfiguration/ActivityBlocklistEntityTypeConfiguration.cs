using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class ActivityBlocklistEntityTypeConfiguration : IEntityTypeConfiguration<ActivityTaskBlocklist>
    {
        public void Configure(EntityTypeBuilder<ActivityTaskBlocklist> builder)
        {
            builder
                .HasKey(x => new { x.GoodplaceUserId, x.Activity });

            builder
                .HasOne<GoodplaceUser>(x => x.GoodplaceUser)
                .WithMany(x => x.ActivityBlocks)
                .HasForeignKey(x => x.GoodplaceUserId)
                .OnDelete(DeleteBehavior.Cascade);
            
        }
    }
}
