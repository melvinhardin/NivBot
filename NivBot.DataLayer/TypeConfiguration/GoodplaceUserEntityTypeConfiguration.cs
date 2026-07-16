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
                .HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
            builder
                .HasIndex(x => x.DiscordUserId)
                .IsUnique();
            
            
        }
    }
}
