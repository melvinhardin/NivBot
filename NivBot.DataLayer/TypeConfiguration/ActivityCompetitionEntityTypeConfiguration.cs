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
                .HasKey(x => x.CompetitionProviderDetailsId);

            builder
                .HasOne<CompetitionProviderDetails>(x => x.CompetitionProviderDetails)
                .WithOne()
                .HasForeignKey<ActivityCompetition>(x => x.CompetitionProviderDetailsId)
                .OnDelete(DeleteBehavior.Cascade);
            builder
                .HasOne<Activity>(x => x.Activity)
                .WithMany()
                .HasForeignKey(x => x.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
