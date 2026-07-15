using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using NivBot.DataLayer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class WalletEntityTypeConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder
                .HasKey(c => c.GoodplaceUserId);

            builder
                .HasOne<GoodplaceUser>(u => u.GoodplaceUser)
                .WithOne(w => w.Wallet)
                .HasForeignKey<Wallet>(w => w.GoodplaceUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
