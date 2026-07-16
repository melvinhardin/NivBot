using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class CompetitionEntityTypeConfiguration :IEntityTypeConfiguration<Competition>
    {
        public void Configure(EntityTypeBuilder<Competition> builder)
        {
            builder
                .HasKey(x => new { x.ExternalId, x.CompetitionProvider });
        }
    }
}
