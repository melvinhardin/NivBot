using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

using NivBot.DataLayer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class CollectableEntityTypeConfiguration : IEntityTypeConfiguration<Collectable>
    {
        public void Configure(EntityTypeBuilder<Collectable> builder)
        {
            builder
                .HasKey(c => c.Id);
            builder
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();

            builder
                .Property(c => c.OsrsName)
                .IsRequired();

            builder
                .Property(c => c.Points)
                .IsRequired();
            
            builder
                .ToTable(c => {
                    c.HasCheckConstraint("CK_Collectable_OsrsName", "length(osrs_name) <= 100");
                    c.HasCheckConstraint("CK_Collectable_Points", "points >= 0 AND points <= 1000");
                });

        }
    }
}
