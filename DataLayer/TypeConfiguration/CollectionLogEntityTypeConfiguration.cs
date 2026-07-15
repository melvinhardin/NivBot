using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using NivBot.DataLayer.Models;
using System.Net.NetworkInformation;
using NetCord;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class CollectionLogEntityTypeConfiguration : IEntityTypeConfiguration<CollectionLog>
    {
        public void Configure(EntityTypeBuilder<CollectionLog> builder)
        {
            

            builder
                .HasKey(c => new {c.CollectableId,c.RunescapeId});

            builder
                .Property(c => c.Amount)
                .IsRequired();

            builder
                .HasOne<Collectable>(c => c.Collectable)
                .WithMany(c => c.CollectionLogs)
                .HasForeignKey(c => c.CollectableId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne<RunescapeAccount>(c => c.RunescapeAccount)
                .WithMany(r => r.CollectionLogs)
                .HasForeignKey(c => c.RunescapeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .ToTable(c =>
                    {
                        c.HasCheckConstraint("CK_CollectionLog_Amount", "amount >= 0");
                    }
                );
            
        }
    }
}
