using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class DiscordMessageEntityTypeConfiguration : IEntityTypeConfiguration<DiscordMessage>
    {
        public void Configure(EntityTypeBuilder<DiscordMessage> builder)
        {
            builder
                .HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
            builder
                .HasIndex(x => x.Type)
                .IsUnique();
        }
    }
}
