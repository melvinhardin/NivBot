using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using NivBot.DataLayer.Models;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class RunescapeAccountEntityTypeConfiguration : IEntityTypeConfiguration<RunescapeAccount>
    {
        public void Configure(EntityTypeBuilder<RunescapeAccount> builder)
        {
            builder
                .HasKey(c => c.RunescapeId);
            builder
                .HasIndex(c => c.RunescapeName)
                .IsUnique();
            builder
                .HasOne<GoodplaceUser>(r => r.GoodplaceUser)
                .WithMany(u => u.RunescapeAccounts)
                .HasForeignKey(r => r.GoodplaceUserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder
                .HasMany<CollectionLog>(r => r.CollectionLogs);
        }
    }
}
