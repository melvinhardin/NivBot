using NivBot.DataLayer.Enums;

namespace NivBot.DataLayer.Models
{
    // XP per hour for a skill, valid from StartXp up to the StartXp of the next row for the same skill.
    // Seeded from Wise Old Man (see Seed/WiseOldManRates.cs), safe to tune by hand.
    public class SkillRate
    {
        public Skills Skill { get; set; }
        public int StartXp { get; set; }
        public int XpPerHour { get; set; }
        public required string Method { get; set; }
    }
}
