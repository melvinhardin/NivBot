using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class ActivityCompetitionEntityTypeConfiguration : IEntityTypeConfiguration<ActivityCompetition>
    {
        public void Configure(EntityTypeBuilder<ActivityCompetition> builder)
        {
            builder
                .HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
        }
    }
}
