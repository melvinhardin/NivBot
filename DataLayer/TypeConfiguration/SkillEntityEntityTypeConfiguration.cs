using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class SkillEntityEntityTypeConfiguration : IEntityTypeConfiguration<SkillEntity>
    {
        public void Configure(EntityTypeBuilder<SkillEntity> builder)
        {
            builder
                .HasKey(x => x.Id);

            builder
                .HasIndex(x => x.Name)
                .IsUnique();
            builder
                .HasIndex(x => x.HiscoreIndex)
                .IsUnique();


        }
    }
}
