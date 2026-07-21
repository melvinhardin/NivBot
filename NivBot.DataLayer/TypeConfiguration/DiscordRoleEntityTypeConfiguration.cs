using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using NivBot.DataLayer.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class DiscordRoleEntityTypeConfiguration : IEntityTypeConfiguration<DiscordRole>
    {
        public void Configure(EntityTypeBuilder<DiscordRole> builder)
        {
            builder
                .HasKey(x => x.DiscordId);
        }
    }
}
