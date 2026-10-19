namespace NivBot.Features.Competitions
{
    // Bound from the "Competitions" section of appsettings.json / environment variables.
    public sealed class CompetitionOptions
    {
        // Points (and currency) for 1st, 2nd, 3rd... Setting this in config replaces the default list.
        public int[]? Rewards { get; set; }

        // Wait this long after the end before reading the results, so the last updates are in.
        public TimeSpan ResultsDelay { get; set; } = TimeSpan.FromMinutes(30);

        // A method, not a property, so the configuration binder leaves it alone.
        public int[] GetRewards() => Rewards is { Length: > 0 } ? Rewards : [30, 20, 10];
    }
}
