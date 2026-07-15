using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using NivBot.DataLayer.Models;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class RunescapeStatsEntityTypeConfiguration : IEntityTypeConfiguration<RunescapeStats>
    {
        public void Configure(EntityTypeBuilder<RunescapeStats> builder)
        {
            builder
                .HasOne<RunescapeAccount>(x => x.RunescapeAccount)
                .WithOne(x => x.RunescapeStats)
                .HasForeignKey<RunescapeStats>(x => x.RunescapeAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
