using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;
using NivBot.DataLayer.Enums;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class SkillEntityTypeConfiguration : IEntityTypeConfiguration<Skill>
    {
        public void Configure(EntityTypeBuilder<Skill> builder)
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
                new Skill { Id = Skills.Attack, Name = nameof(Skills.Attack), HiscoreIndex = (int)Skills.Attack },
                new Skill { Id = Skills.Defence, Name = nameof(Skills.Defence), HiscoreIndex = (int)Skills.Defence },
                new Skill { Id = Skills.Strength, Name = nameof(Skills.Strength), HiscoreIndex = (int)Skills.Strength },
                new Skill { Id = Skills.Hitpoints, Name = nameof(Skills.Hitpoints), HiscoreIndex = (int)Skills.Hitpoints },
                new Skill { Id = Skills.Ranged, Name = nameof(Skills.Ranged), HiscoreIndex = (int)Skills.Ranged },
                new Skill { Id = Skills.Prayer, Name = nameof(Skills.Prayer), HiscoreIndex = (int)Skills.Prayer },
                new Skill { Id = Skills.Magic, Name = nameof(Skills.Magic), HiscoreIndex = (int)Skills.Magic },
                new Skill { Id = Skills.Cooking, Name = nameof(Skills.Cooking), HiscoreIndex = (int)Skills.Cooking },
                new Skill { Id = Skills.Woodcutting, Name = nameof(Skills.Woodcutting), HiscoreIndex = (int)Skills.Woodcutting },
                new Skill { Id = Skills.Fletching, Name = nameof(Skills.Fletching), HiscoreIndex = (int)Skills.Fletching },
                new Skill { Id = Skills.Fishing, Name = nameof(Skills.Fishing), HiscoreIndex = (int)Skills.Fishing },
                new Skill { Id = Skills.Firemaking, Name = nameof(Skills.Firemaking), HiscoreIndex = (int)Skills.Firemaking },
                new Skill { Id = Skills.Crafting, Name = nameof(Skills.Crafting), HiscoreIndex = (int)Skills.Crafting },
                new Skill { Id = Skills.Smithing, Name = nameof(Skills.Smithing), HiscoreIndex = (int)Skills.Smithing },
                new Skill { Id = Skills.Mining, Name = nameof(Skills.Mining), HiscoreIndex = (int)Skills.Mining },
                new Skill { Id = Skills.Herblore, Name = nameof(Skills.Herblore), HiscoreIndex = (int)Skills.Herblore },
                new Skill { Id = Skills.Agility, Name = nameof(Skills.Agility), HiscoreIndex = (int)Skills.Agility },
                new Skill { Id = Skills.Thieving, Name = nameof(Skills.Thieving), HiscoreIndex = (int)Skills.Thieving },
                new Skill { Id = Skills.Slayer, Name = nameof(Skills.Slayer), HiscoreIndex = (int)Skills.Slayer },
                new Skill { Id = Skills.Farming, Name = nameof(Skills.Farming), HiscoreIndex = (int)Skills.Farming },
                new Skill { Id = Skills.Runecrafting, Name = nameof(Skills.Runecrafting), HiscoreIndex = (int)Skills.Runecrafting },
                new Skill { Id = Skills.Hunter, Name = nameof(Skills.Hunter), HiscoreIndex = (int)Skills.Hunter },
                new Skill { Id = Skills.Construction, Name = nameof(Skills.Construction), HiscoreIndex = (int)Skills.Construction },
                new Skill { Id = Skills.Sailing, Name = nameof(Skills.Sailing), HiscoreIndex = (int)Skills.Sailing }
            );


        }
    }
}
