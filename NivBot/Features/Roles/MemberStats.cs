using NivBot.DataLayer.Enums;
using NivBot.DataLayer.Models;
using NivBot.Features.Shared;

namespace NivBot.Features.Roles
{
    // Everything role rules can measure for one member.
    // KC-like values are added up over the member's linked accounts, level-like values use the best account.
    public sealed class MemberStats
    {
        private readonly Dictionary<int, long> activityScores = new();
        private readonly Dictionary<Skills, int> skillLevels = new();
        private readonly Dictionary<int, long> collectionLogItems;
        private long totalLevel, totalXp, totalBossKc, raidsCompleted, skillsAt99, skillsAt200m;

        // accounts: with RunescapeStats and ActivityLogs loaded.
        // bossActivities: activity id -> is a raid, for every activity that is a boss.
        // collectionLogItems: internal item id -> amount over all accounts.
        public MemberStats(IEnumerable<RunescapeAccount> accounts, IReadOnlyDictionary<int, bool> bossActivities, Dictionary<int, long> collectionLogItems)
        {
            this.collectionLogItems = collectionLogItems;

            foreach (RunescapeAccount account in accounts)
            {
                totalLevel = Math.Max(totalLevel, account.OverallLevel);
                totalXp = Math.Max(totalXp, account.OverallXp);

                List<RunescapeStat> stats = account.RunescapeStats?.ToList() ?? new List<RunescapeStat>();
                skillsAt99 = Math.Max(skillsAt99, stats.Count(x => x.Xp >= OsrsLevels.Level99Xp));
                skillsAt200m = Math.Max(skillsAt200m, stats.Count(x => x.Xp >= OsrsLevels.MaxXp));
                foreach (RunescapeStat stat in stats)
                {
                    int level = OsrsLevels.FromXp(stat.Xp);
                    skillLevels[stat.Skill] = Math.Max(skillLevels.GetValueOrDefault(stat.Skill), level);
                }

                foreach (ActivityLog log in account.ActivityLogs ?? new List<ActivityLog>())
                {
                    activityScores[log.ActivityId] = activityScores.GetValueOrDefault(log.ActivityId) + log.Amount;
                    if (bossActivities.TryGetValue(log.ActivityId, out bool isRaid))
                    {
                        totalBossKc += log.Amount;
                        if (isRaid) { raidsCompleted += log.Amount; }
                    }
                }
            }
        }

        public long Value(DiscordRole rule) => rule.Metric switch
        {
            RoleMetric.ActivityScore => rule.ActivityId is int activityId ? activityScores.GetValueOrDefault(activityId) : 0,
            RoleMetric.TotalLevel => totalLevel,
            RoleMetric.TotalXp => totalXp,
            RoleMetric.TotalBossKc => totalBossKc,
            RoleMetric.RaidsCompleted => raidsCompleted,
            RoleMetric.SkillsAt99 => skillsAt99,
            RoleMetric.SkillsAt200m => skillsAt200m,
            RoleMetric.SkillLevel => rule.Skill is Skills skill ? skillLevels.GetValueOrDefault(skill) : 0,
            RoleMetric.CollectionLogItem => rule.ItemId is int itemId ? collectionLogItems.GetValueOrDefault(itemId) : 0,
            _ => 0
        };

        // What the number in Value means, for posts and lists. Needs Activity / Item loaded on the rule.
        public static string Describe(DiscordRole rule) => rule.Metric switch
        {
            RoleMetric.ActivityScore => rule.Activity?.OsrsName ?? "KC",
            RoleMetric.TotalLevel => "total level",
            RoleMetric.TotalXp => "total XP",
            RoleMetric.TotalBossKc => "total boss KC",
            RoleMetric.RaidsCompleted => "raids completed",
            RoleMetric.SkillsAt99 => "skills at 99",
            RoleMetric.SkillsAt200m => "skills at 200m XP",
            RoleMetric.SkillLevel => $"{rule.Skill} level",
            RoleMetric.CollectionLogItem => rule.Item?.OsrsName ?? "item",
            _ => rule.Metric.ToString()
        };
    }
}
