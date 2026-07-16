using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class SkillCompetitionEntityTypeConfiguration : IEntityTypeConfiguration<SkillCompetition>
    {
        public void Configure(EntityTypeBuilder<SkillCompetition> builder)
        {
            builder
                .HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
        }
    }
}
