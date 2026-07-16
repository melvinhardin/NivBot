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
                .HasOne<CompetitionProviderDetails>(x => x.CompetitionProviderDetails)
                .WithOne()
                .HasForeignKey<SkillCompetition>(x => x.CompetitionProviderDetailsId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
