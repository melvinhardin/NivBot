using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;
using NivBot.DataLayer.Enums;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class SkillEntityEntityTypeConfiguration : IEntityTypeConfiguration<SkillEntity>
    {
        public void Configure(EntityTypeBuilder<SkillEntity> builder)
        {
            builder
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .ValueGeneratedNever();

            builder
                .HasIndex(x => x.Name)
                .IsUnique();
            builder
                .HasIndex(x => x.HiscoreIndex)
                .IsUnique();

            builder.HasData(
                new SkillEntity { Id = (int)Skills.Attack, Name = nameof(Skills.Attack), HiscoreIndex = (int)Skills.Attack },
                new SkillEntity { Id = (int)Skills.Defence, Name = nameof(Skills.Defence), HiscoreIndex = (int)Skills.Defence },
                new SkillEntity { Id = (int)Skills.Strength, Name = nameof(Skills.Strength), HiscoreIndex = (int)Skills.Strength },
                new SkillEntity { Id = (int)Skills.Hitpoints, Name = nameof(Skills.Hitpoints), HiscoreIndex = (int)Skills.Hitpoints },
                new SkillEntity { Id = (int)Skills.Ranged, Name = nameof(Skills.Ranged), HiscoreIndex = (int)Skills.Ranged },
                new SkillEntity { Id = (int)Skills.Prayer, Name = nameof(Skills.Prayer), HiscoreIndex = (int)Skills.Prayer },
                new SkillEntity { Id = (int)Skills.Magic, Name = nameof(Skills.Magic), HiscoreIndex = (int)Skills.Magic },
                new SkillEntity { Id = (int)Skills.Cooking, Name = nameof(Skills.Cooking), HiscoreIndex = (int)Skills.Cooking },
                new SkillEntity { Id = (int)Skills.Woodcutting, Name = nameof(Skills.Woodcutting), HiscoreIndex = (int)Skills.Woodcutting },
                new SkillEntity { Id = (int)Skills.Fletching, Name = nameof(Skills.Fletching), HiscoreIndex = (int)Skills.Fletching },
                new SkillEntity { Id = (int)Skills.Fishing, Name = nameof(Skills.Fishing), HiscoreIndex = (int)Skills.Fishing },
                new SkillEntity { Id = (int)Skills.Firemaking, Name = nameof(Skills.Firemaking), HiscoreIndex = (int)Skills.Firemaking },
                new SkillEntity { Id = (int)Skills.Crafting, Name = nameof(Skills.Crafting), HiscoreIndex = (int)Skills.Crafting },
                new SkillEntity { Id = (int)Skills.Smithing, Name = nameof(Skills.Smithing), HiscoreIndex = (int)Skills.Smithing },
                new SkillEntity { Id = (int)Skills.Mining, Name = nameof(Skills.Mining), HiscoreIndex = (int)Skills.Mining },
                new SkillEntity { Id = (int)Skills.Herblore, Name = nameof(Skills.Herblore), HiscoreIndex = (int)Skills.Herblore },
                new SkillEntity { Id = (int)Skills.Agility, Name = nameof(Skills.Agility), HiscoreIndex = (int)Skills.Agility },
                new SkillEntity { Id = (int)Skills.Thieving, Name = nameof(Skills.Thieving), HiscoreIndex = (int)Skills.Thieving },
                new SkillEntity { Id = (int)Skills.Slayer, Name = nameof(Skills.Slayer), HiscoreIndex = (int)Skills.Slayer },
                new SkillEntity { Id = (int)Skills.Farming, Name = nameof(Skills.Farming), HiscoreIndex = (int)Skills.Farming },
                new SkillEntity { Id = (int)Skills.Runecrafting, Name = nameof(Skills.Runecrafting), HiscoreIndex = (int)Skills.Runecrafting },
                new SkillEntity { Id = (int)Skills.Hunter, Name = nameof(Skills.Hunter), HiscoreIndex = (int)Skills.Hunter },
                new SkillEntity { Id = (int)Skills.Construction, Name = nameof(Skills.Construction), HiscoreIndex = (int)Skills.Construction },
                new SkillEntity { Id = (int)Skills.Sailing, Name = nameof(Skills.Sailing), HiscoreIndex = (int)Skills.Sailing }
            );


        }
    }
}
