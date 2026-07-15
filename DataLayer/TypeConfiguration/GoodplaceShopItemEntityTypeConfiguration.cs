using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class GoodplaceShopItemEntityTypeConfiguration : IEntityTypeConfiguration<GoodplaceShopItem>
    {
        public void Configure(EntityTypeBuilder<GoodplaceShopItem> builder)
        {
            builder
                .HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
            builder
                .ToTable<GoodplaceShopItem>(t => t.HasCheckConstraint("CK_GoodplaceShopItem_Price", "price >= 0"));
        }
    }
}
