using NivBot.DataLayer.Models;

namespace NivBot.Features.GoodplaceTask
{
    // Sizes tasks so they take roughly TaskOptions.MinHours - MaxHours at the (scaled) seeded rates.
    public sealed class TaskGoalCalculator(TaskOptions options)
    {
        private const long MaxXp = 200_000_000;

        public double RollHours()
        {
            double min = Math.Max(0.1, Math.Min(options.MinHours, options.MaxHours));
            double max = Math.Max(min, Math.Max(options.MinHours, options.MaxHours));
            return min + Random.Shared.NextDouble() * (max - min);
        }

        // XP gained by training for `hours` starting at currentXp. Walks through the XP/h brackets of the skill,
        // so a task that crosses into a faster method gets the faster rate for the rest of the time.
        // Returns 0 when the skill has no usable rates or is already at 200m.
        public long XpToGain(IEnumerable<SkillRate> skillRates, long currentXp, double hours)
        {
            List<SkillRate> brackets = skillRates
                .Where(x => x.XpPerHour > 0)
                .OrderBy(x => x.StartXp)
                .ToList();
            if (brackets.Count == 0) { return 0; }

            double start = Math.Clamp(currentXp, 0, MaxXp);
            double xp = start;
            double hoursLeft = hours;

            int i = 0;
            while (i + 1 < brackets.Count && brackets[i + 1].StartXp <= xp) { i++; }

            while (hoursLeft > 0 && xp < MaxXp)
            {
                double rate = brackets[i].XpPerHour * options.SkillRateMultiplier;
                if (rate <= 0) { break; }

                bool lastBracket = i + 1 >= brackets.Count;
                double bracketEnd = lastBracket ? MaxXp : brackets[i + 1].StartXp;
                double hoursInBracket = (bracketEnd - xp) / rate;

                if (hoursInBracket >= hoursLeft)
                {
                    xp += rate * hoursLeft;
                    hoursLeft = 0;
                }
                else
                {
                    xp = bracketEnd;
                    hoursLeft -= hoursInBracket;
                    if (lastBracket) { break; }
                    i++;
                }
            }

            // Round up to a whole 100 XP so goals read nicely, without going past 200m.
            long gained = (long)(Math.Ceiling((Math.Min(xp, MaxXp) - start) / 100.0) * 100);
            return Math.Min(gained, MaxXp - (long)start);
        }

        public int KillsToGet(Boss boss, int currentKc, double hours)
        {
            int kills = (int)Math.Round(boss.KillsPerHour * options.BossRateMultiplier * hours, MidpointRounding.AwayFromZero);
            kills = Math.Max(kills, 1);

            // Kills below the hiscore minimum don't show up, so the goal has to reach at least the minimum.
            if (currentKc < boss.MinimumKc)
            {
                kills = Math.Max(kills, boss.MinimumKc - currentKc);
            }
            return kills;
        }
    }
}
