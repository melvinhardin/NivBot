using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class CompetitionProviderDetailsEntityTypeConfiguration : IEntityTypeConfiguration<CompetitionProviderDetails>
    {
        public void Configure(EntityTypeBuilder<CompetitionProviderDetails> builder)
        {
            builder
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder
                .HasIndex(x => new { x.ExternalId, x.CompetitionProvider })
                .IsUnique();
            
        }
    }
}
