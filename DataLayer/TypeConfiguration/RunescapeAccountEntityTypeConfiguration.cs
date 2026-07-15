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
                .HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
            builder
                .HasIndex(x => x.RunescapeName)
                .IsUnique();
            builder
                .HasOne<GoodplaceUser>(x => x.GoodplaceUser)
                .WithMany(x => x.RunescapeAccounts)
                .HasForeignKey(x => x.GoodplaceUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
