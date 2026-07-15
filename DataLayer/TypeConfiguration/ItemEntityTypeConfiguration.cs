using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

using NivBot.DataLayer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class ItemEntityTypeConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder
                .HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
            builder
                .Property(x => x.OsrsName)
                .IsRequired();

            builder
                .Property(x => x.Points);
            
            builder
                .ToTable(t => {
                    t.HasCheckConstraint("CK_Collectable_OsrsName", "length(osrs_name) <= 100");
                    t.HasCheckConstraint("CK_Collectable_Points", "points >= 0 AND points <= 1000");
                });

        }
    }
}
