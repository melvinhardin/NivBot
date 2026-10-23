using NivBot.DataLayer.Enums;

namespace NivBot.Features.GoodplaceTask
{
    // Bound from the "Tasks" section of appsettings.json / environment variables (Tasks__MinHours etc.).
    public sealed class TaskOptions
    {
        // A task is sized to take a random amount of hours between these two values.
        public double MinHours { get; set; } = 1.5;
        public double MaxHours { get; set; } = 2.5;

        // The seeded Wise Old Man rates are efficient-play rates, these scale them down to casual play.
        public double SkillRateMultiplier { get; set; } = 0.5;
        public double BossRateMultiplier { get; set; } = 0.5;

        public int PointsPerTask { get; set; } = 5;
        public int CurrencyPerTask { get; set; } = 5;

        // Per task type (skill / boss). Format "hh:mm:ss" or "d.hh:mm:ss".
        public TimeSpan SkipCooldown { get; set; } = TimeSpan.FromHours(24);

        public int MaxSkillBlocks { get; set; } = 3;
        public int MaxBossBlocks { get; set; } = 10;

        // Never assigned as skill tasks. Setting this in config replaces the default list.
        // Farming is excluded because its rates are tree runs, which are time-gated and can't be done in ~2 hours.
        public Skills[]? ExcludedSkills { get; set; }

        // A method, not a property, so the configuration binder leaves it alone.
        public IReadOnlyCollection<Skills> GetExcludedSkills() => ExcludedSkills ?? DefaultExcludedSkills;

        private static readonly Skills[] DefaultExcludedSkills =
        [
            Skills.Attack, Skills.Defence, Skills.Strength, Skills.Magic, Skills.Hitpoints, Skills.Ranged, Skills.Prayer,
            Skills.Farming
        ];
    }
}
