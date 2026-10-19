namespace NivBot.DataLayer.Models
{
    // Everything the bot knows about a boss, matched to Activity.OsrsName (case insensitive).
    // Activities without a row here (clues, minigames, LMS etc.) are not bosses: they are never boss tasks
    // and don't count towards total boss KC.
    // Seeded from Wise Old Man (see Seed/WiseOldManRates.cs), safe to tune by hand.
    public class Boss
    {
        public required string OsrsName { get; set; }

        // 0 means the boss is never assigned as a boss task.
        public double KillsPerHour { get; set; }

        // Kill count a player needs before the hiscores show the boss at all.
        public int MinimumKc { get; set; }

        // Counts towards "raids completed".
        public bool IsRaid { get; set; }

        // Metric keys used when creating competitions.
        public string? WomMetric { get; set; }
        public string? TempleMetric { get; set; }
    }
}
