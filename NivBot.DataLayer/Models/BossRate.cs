namespace NivBot.DataLayer.Models
{
    // Kills per hour for a boss. Matched to Activity.OsrsName (case insensitive), so activities
    // without a row here (clues, minigames, LMS etc.) are never assigned as boss tasks.
    // Seeded from Wise Old Man (see Seed/WiseOldManRates.cs), safe to tune by hand.
    public class BossRate
    {
        public required string OsrsName { get; set; }
        public double KillsPerHour { get; set; }

        // Kill count a player needs before the hiscores show the boss at all.
        public int MinimumKc { get; set; }
    }
}
