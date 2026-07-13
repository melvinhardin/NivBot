using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using NivBot.DataLayer.Models;
using System.Net.NetworkInformation;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class CollectionLogEntityTypeConfiguration : IEntityTypeConfiguration<CollectionLog>
    {
        public void Configure(EntityTypeBuilder<CollectionLog> builder)
        {
            

            builder.HasKey(c => new {c.CollectableId,c.RunescapeAccountName});


            builder.Property(c => c.Ammount)
                .IsRequired();

            builder.ToTable(c =>
            {
                c.HasCheckConstraint("CK_CollectionLog_Ammount", "ammount => 0");
            });
        }
    }
}
