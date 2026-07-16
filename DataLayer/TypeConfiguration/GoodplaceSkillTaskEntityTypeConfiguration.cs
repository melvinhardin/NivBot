using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class GoodplaceSkillTaskEntityTypeConfiguration : IEntityTypeConfiguration<GoodplaceSkillTask>
    {
        public void Configure(EntityTypeBuilder<GoodplaceSkillTask> builder)
        {
            builder
                .HasKey(x => new { x.GoodplaceUserId, x.Skill });

            builder
                .HasOne<GoodplaceUser>(x => x.GoodplaceUser)
                .WithOne(x => x.GoodplaceSkillTask)
                .HasForeignKey<GoodplaceSkillTask>(x => x.GoodplaceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne<SkillEntity>()
                .WithMany()
                .HasForeignKey(x => x.Skill)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
