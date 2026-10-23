using NivBot.DataLayer.Enums;

namespace NivBot.DataLayer.Models
{
    // A Discord role the bot hands out when a member reaches Threshold on Metric.
    public class DiscordRole
    {
        public long DiscordRoleId { get; set; }
        public long GuildId { get; set; }

        // Shown in /roles list and the "close to" posts, e.g. "Vorkath Expert".
        public required string Name { get; set; }

        public RoleMetric Metric { get; set; }

        // Target of the metric, depending on Metric: ActivityScore -> ActivityId, SkillLevel -> Skill, CollectionLogItem -> ItemId.
        public int? ActivityId { get; set; }
        public Activity? Activity { get; set; }
        public Skills? Skill { get; set; }
        public int? ItemId { get; set; }
        public Item? Item { get; set; }

        public long Threshold { get; set; }

        // When set, the bot posts once that a member is close to the role when they pass this value.
        public long? CloseTrigger { get; set; }

        // Roles with the same tier group are tiers of one badge: a member only keeps the highest one they qualify for.
        public string? TierGroup { get; set; }

        // Custom emoji shown in posts, e.g. <:CollectorBadge:926004335014457456>
        public string? Emoji { get; set; }
    }
}
