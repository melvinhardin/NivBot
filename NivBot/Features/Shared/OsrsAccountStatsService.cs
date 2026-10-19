using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NivBot.DataLayer;
using NivBot.DataLayer.Enums;
using NivBot.DataLayer.Models;
using NivBot.ExternalServicesLayer.OsrsAPI;
using System.Text.Json;
// Not importing the OsrsAPI.Models namespace: its Skills class would clash with the Skills enum.
using PlayerStats = NivBot.ExternalServicesLayer.OsrsAPI.Models.PlayerStats;

namespace NivBot.Features.Shared
{
    public enum StatsRefreshResult
    {
        Success,
        HiscoresUnavailable
    }

    // Keeps RunescapeStats and ActivityLogs in line with the OSRS hiscores.
    public class OsrsAccountStatsService(GoodplaceContext db, IOsrsHighscoreService hiscores, ILogger<OsrsAccountStatsService> logger)
    {
        public const int MaxXp = 200_000_000;

        // Loads every account of the user (tracked, with stats and activity logs) and updates them from the hiscores.
        // Does not save: the caller saves together with its own changes.
        public async Task<(StatsRefreshResult Result, List<RunescapeAccount> Accounts)> RefreshUserAccountsAsync(int goodplaceUserId)
        {
            List<RunescapeAccount> accounts = await db.RunescapeAccounts
                .Where(x => x.GoodplaceUserId == goodplaceUserId)
                .Include(x => x.RunescapeStats)
                .Include(x => x.ActivityLogs)
                .AsSplitQuery()
                .ToListAsync();

            if (accounts.Count == 0) { return (StatsRefreshResult.Success, accounts); }

            Dictionary<string, int> activityIds = await GetActivityIdsByNameAsync();

            foreach (RunescapeAccount account in accounts)
            {
                PlayerStats? stats;
                try
                {
                    stats = await hiscores.GetPlayerStatsAsync(account.RunescapeName);
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
                {
                    logger.LogWarning(ex, "Hiscores lookup failed for {Account}", account.RunescapeName);
                    return (StatsRefreshResult.HiscoresUnavailable, accounts);
                }

                // Renamed or banned accounts drop off the hiscores, keep their last known stats.
                if (stats == null)
                {
                    logger.LogInformation("{Account} is not on the hiscores anymore, keeping its last known stats", account.RunescapeName);
                    continue;
                }

                ApplyStats(account, stats, activityIds);
            }

            return (StatsRefreshResult.Success, accounts);
        }

        public Task<Dictionary<string, int>> GetActivityIdsByNameAsync() =>
            db.Activities.ToDictionaryAsync(x => x.OsrsName, x => x.Id);

        // Copies hiscores values onto the account.
        // The hiscores return -1 for anything unranked: new rows store 0, existing values are never overwritten by -1.
        // Activities that are not in the Activities table yet are skipped (an admin adds them with /syncactivities).
        public static void ApplyStats(RunescapeAccount account, PlayerStats stats, IReadOnlyDictionary<string, int> activityIds)
        {
            account.RunescapeStats ??= new List<RunescapeStat>();
            account.ActivityLogs ??= new List<ActivityLog>();

            // Overall (id 0): total level and total XP. Unranked overall comes back as -1 or 0, keep the last known value then.
            var overall = stats.Skills.FirstOrDefault(x => x.Id == 0);
            if (overall != null && overall.Level > 0 && overall.Xp > 0)
            {
                account.OverallLevel = overall.Level;
                account.OverallXp = overall.Xp;
            }

            foreach (Skills skill in Enum.GetValues<Skills>())
            {
                // The hiscores skill id is the same number as the Skills enum (Overall is 0).
                var apiSkill = stats.Skills.FirstOrDefault(x => x.Id == (int)skill);
                if (apiSkill == null) { continue; }

                int xp = (int)Math.Clamp(apiSkill.Xp, -1L, MaxXp);
                RunescapeStat? stat = account.RunescapeStats.FirstOrDefault(x => x.Skill == skill);
                if (stat == null)
                {
                    account.RunescapeStats.Add(new RunescapeStat { Skill = skill, Xp = Math.Max(xp, 0) });
                }
                else if (xp >= 0)
                {
                    stat.Xp = xp;
                }
            }

            foreach (var apiActivity in stats.Activities)
            {
                if (!activityIds.TryGetValue(apiActivity.Name, out int activityId)) { continue; }

                ActivityLog? log = account.ActivityLogs.FirstOrDefault(x => x.ActivityId == activityId);
                if (log == null)
                {
                    account.ActivityLogs.Add(new ActivityLog { ActivityId = activityId, Amount = Math.Max(apiActivity.Score, 0) });
                }
                else if (apiActivity.Score >= 0)
                {
                    log.Amount = apiActivity.Score;
                }
            }
        }
    }
}
