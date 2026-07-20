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
                    t.HasCheckConstraint("CK_Item_OsrsName", "length(\"OsrsName\") <= 100");
                    t.HasCheckConstraint("CK_Item_Points", "\"Points\" >= 0 AND \"Points\" <= 1000");
                });

        }
    }
}
