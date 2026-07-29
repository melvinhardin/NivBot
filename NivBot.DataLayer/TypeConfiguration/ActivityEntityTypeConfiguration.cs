using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

using NivBot.DataLayer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class ActivityEntityTypeConfiguration : IEntityTypeConfiguration<Activity>
    {
        public void Configure(EntityTypeBuilder<Activity> builder)
        {
            builder
                .HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
            builder
                .HasIndex(x => x.OsrsName)
                .IsUnique();

            builder
                .Property(x => x.Points);
            
            builder
                .ToTable(t => {
                    t.HasCheckConstraint("CK_Activity_OsrsName", "length(\"OsrsName\") <= 100");
                    t.HasCheckConstraint("CK_Activity_Points", "\"Points\" >= 0 AND \"Points\" <= 1000");
                });

        }
    }
}
