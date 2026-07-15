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
                .HasKey(x => x.GoodplaceUserId);

            builder
                .HasOne<GoodplaceUser>(x => x.GoodplaceUser)
                .WithOne(x => x.Wallet)
                .HasForeignKey<Wallet>(x => x.GoodplaceUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
