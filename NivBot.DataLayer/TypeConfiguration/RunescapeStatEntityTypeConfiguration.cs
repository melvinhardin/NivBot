using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using NivBot.DataLayer.Models;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class RunescapeStatEntityTypeConfiguration : IEntityTypeConfiguration<RunescapeStat>
    {
        public void Configure(EntityTypeBuilder<RunescapeStat> builder)
        {
            builder
                .HasKey(x => new { x.RunescapeAccountId, x.Skill });

            builder
                .HasOne<RunescapeAccount>(x => x.RunescapeAccount)
                .WithOne(x => x.RunescapeStat)
                .HasForeignKey<RunescapeStat>(x => x.RunescapeAccountId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder
                .HasOne<Skill>()
                .WithMany()
                .HasForeignKey(x => x.Skill)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .ToTable(t => t.HasCheckConstraint("CK_RunescapeStat_Xp", "\"Xp\" >= 0 AND \"Xp\" < 200000000"));
        }
    }
}
