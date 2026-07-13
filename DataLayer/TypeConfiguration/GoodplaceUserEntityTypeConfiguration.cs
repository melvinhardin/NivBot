using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;


namespace NivBot.DataLayer.TypeConfiguration
{
    public class GoodplaceUserEntityTypeConfiguration : IEntityTypeConfiguration<GoodplaceUser>
    {
        public void Configure(EntityTypeBuilder<GoodplaceUser> builder)
        {
            builder
                .HasKey(c => c.Id);
            builder
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();
            builder
                .HasIndex(c => c.DiscordUserId)
                .IsUnique();
            builder
                .Property(c => c.DiscordUserId)
                .IsRequired();
            
        }
    }
}
