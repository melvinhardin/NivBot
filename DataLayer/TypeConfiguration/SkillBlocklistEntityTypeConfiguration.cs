using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class SkillBlocklistEntityTypeConfiguration : IEntityTypeConfiguration<SkillTaskBlocklist>
    {
        public void Configure(EntityTypeBuilder<SkillTaskBlocklist> builder)
        {
            builder
                .HasKey(x => new { x.GoodplaceUserId, x.Skill });

            builder
                .HasOne<GoodplaceUser>(x => x.GoodplaceUser)
                .WithMany(x => x.SkillBlocks)
                .HasForeignKey(x => x.GoodplaceUserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder
                .HasOne<Skill>()
                .WithMany()
                .HasForeignKey(x => x.Skill)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
