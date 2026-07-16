using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class GlobalActivityBlocklistEntityTypeConfiguration : IEntityTypeConfiguration<GlobalActivityBlocklist>
    {
        public void Configure(EntityTypeBuilder<GlobalActivityBlocklist> builder)
        {
            builder
                .HasKey(x => x.ActivityId);
            builder
                .HasOne<Activity>(x => x.Activity)
                .WithOne()
                .HasForeignKey<GlobalActivityBlocklist>(x => x.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
