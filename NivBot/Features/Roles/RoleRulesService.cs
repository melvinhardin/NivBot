using Microsoft.EntityFrameworkCore;
using NivBot.DataLayer;
using NivBot.DataLayer.Enums;
using NivBot.DataLayer.Models;
using NivBot.Features.Shared;
using System.Globalization;

namespace NivBot.Features.Roles
{
    // A rule as given by an admin (command or import row), before validation.
    public sealed record RoleRuleInput(
        long RoleId,
        string Name,
        RoleMetric Metric,
        long Threshold,
        string? Activity = null,
        Skills? Skill = null,
        string? Item = null,
        long? CloseTrigger = null,
        string? TierGroup = null,
        string? Emoji = null);

    public sealed record RoleImportResult(int Saved, List<string> Problems);

    // Managing role rules for /roles.
    public class RoleRulesService(GoodplaceContext db)
    {
        public const string ImportHeader = "role_id,name,metric,target,threshold,close_trigger,tier_group,emoji";

        // Adds or replaces the rule for the role. Returns an error message, or null when saved.
        public async Task<string?> SaveAsync(long guildId, RoleRuleInput input)
        {
            var (rule, error) = await BuildAsync(guildId, input);
            if (rule == null) { return error; }

            DiscordRole? existing = await db.DiscordRoles.FirstOrDefaultAsync(x => x.DiscordRoleId == input.RoleId);
            if (existing != null)
            {
                db.DiscordRoles.Remove(existing);
                // A changed rule starts over with its close-to posts
                await db.RoleProgressNotices.Where(x => x.DiscordRoleId == input.RoleId).ExecuteDeleteAsync();
                await db.SaveChangesAsync();
            }

            db.DiscordRoles.Add(rule);
            await db.SaveChangesAsync();
            return null;
        }

        public async Task<bool> RemoveAsync(long guildId, long roleId)
        {
            int removed = await db.DiscordRoles
                .Where(x => x.GuildId == guildId && x.DiscordRoleId == roleId)
                .ExecuteDeleteAsync();
            return removed > 0;
        }

        public Task<List<DiscordRole>> ListAsync(long guildId) =>
            db.DiscordRoles
                .AsNoTracking()
                .Include(x => x.Activity)
                .Include(x => x.Item)
                .Where(x => x.GuildId == guildId)
                .OrderBy(x => x.Metric)
                .ThenBy(x => x.TierGroup)
                .ThenBy(x => x.Threshold)
                .ToListAsync();

        // Imports a CSV (see roles-import.csv in the repository) with the columns in ImportHeader.
        // serverRoles: role id -> name of every role in the server, rows with unknown role ids are rejected.
        public async Task<RoleImportResult> ImportAsync(long guildId, string csv, IReadOnlyDictionary<long, string> serverRoles)
        {
            var problems = new List<string>();
            int saved = 0;

            List<List<string>> rows = Csv.Parse(csv.TrimStart('﻿'));
            if (rows.Count > 0 && rows[0].Count > 0 && rows[0][0].Equals("role_id", StringComparison.OrdinalIgnoreCase))
            {
                rows.RemoveAt(0);
            }

            for (int i = 0; i < rows.Count; i++)
            {
                List<string> row = rows[i];
                string line = $"line {i + 2}";
                string Column(int index) => index < row.Count ? row[index] : "";

                string name = Column(1);
                if (Column(0).Length == 0)
                {
                    problems.Add($"{line} ({name}): no role id, skipped");
                    continue;
                }
                if (!long.TryParse(Column(0), NumberStyles.None, CultureInfo.InvariantCulture, out long roleId) || !serverRoles.ContainsKey(roleId))
                {
                    problems.Add($"{line} ({name}): role {Column(0)} doesn't exist in this server");
                    continue;
                }
                if (!Enum.TryParse(Column(2), ignoreCase: true, out RoleMetric metric) || !Enum.IsDefined(metric))
                {
                    problems.Add($"{line} ({name}): unknown metric '{Column(2)}'");
                    continue;
                }
                if (!long.TryParse(Column(4), NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out long threshold))
                {
                    problems.Add($"{line} ({name}): threshold '{Column(4)}' isn't a number");
                    continue;
                }

                long? closeTrigger = null;
                if (Column(5).Length > 0)
                {
                    if (!long.TryParse(Column(5), NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out long close))
                    {
                        problems.Add($"{line} ({name}): close_trigger '{Column(5)}' isn't a number");
                        continue;
                    }
                    closeTrigger = close;
                }

                string target = Column(3);
                Skills? skill = null;
                if (metric == RoleMetric.SkillLevel)
                {
                    if (!Enum.TryParse(target, ignoreCase: true, out Skills parsedSkill) || !Enum.IsDefined(parsedSkill))
                    {
                        problems.Add($"{line} ({name}): unknown skill '{target}'");
                        continue;
                    }
                    skill = parsedSkill;
                }

                var input = new RoleRuleInput(
                    roleId,
                    name.Length > 0 ? name : serverRoles[roleId],
                    metric,
                    threshold,
                    Activity: metric == RoleMetric.ActivityScore ? target : null,
                    Skill: skill,
                    Item: metric == RoleMetric.CollectionLogItem ? target : null,
                    CloseTrigger: closeTrigger,
                    TierGroup: Column(6),
                    Emoji: Column(7));

                string? error = await SaveAsync(guildId, input);
                if (error != null)
                {
                    problems.Add($"{line} ({name}): {error}");
                    continue;
                }
                saved++;
            }

            return new RoleImportResult(saved, problems);
        }

        private async Task<(DiscordRole? Rule, string? Error)> BuildAsync(long guildId, RoleRuleInput input)
        {
            string name = input.Name.Trim();
            if (name.Length == 0 || name.Length > 100) { return (null, "the name must be 1-100 characters"); }
            if (input.Threshold < 0) { return (null, "the threshold can't be negative"); }
            if (input.CloseTrigger is long close && (close < 0 || close >= input.Threshold))
            {
                return (null, "the close trigger must be below the threshold");
            }

            string? tierGroup = string.IsNullOrWhiteSpace(input.TierGroup) ? null : input.TierGroup.Trim();
            if (tierGroup?.Length > 50) { return (null, "the tier group can be at most 50 characters"); }
            string? emoji = string.IsNullOrWhiteSpace(input.Emoji) ? null : input.Emoji.Trim();
            if (emoji?.Length > 100) { return (null, "the emoji can be at most 100 characters"); }

            var rule = new DiscordRole
            {
                DiscordRoleId = input.RoleId,
                GuildId = guildId,
                Name = name,
                Metric = input.Metric,
                Threshold = input.Threshold,
                CloseTrigger = input.CloseTrigger,
                TierGroup = tierGroup,
                Emoji = emoji
            };

            switch (input.Metric)
            {
                case RoleMetric.ActivityScore:
                {
                    string search = (input.Activity ?? "").Trim().ToLower();
                    if (search.Length == 0) { return (null, "this metric needs a boss / activity"); }
                    int? activityId = await db.Activities
                        .Where(x => x.OsrsName.ToLower() == search)
                        .Select(x => (int?)x.Id)
                        .FirstOrDefaultAsync();
                    if (activityId == null) { return (null, $"activity '{input.Activity}' not found (run /syncactivities first)"); }
                    rule.ActivityId = activityId;
                    break;
                }
                case RoleMetric.SkillLevel:
                {
                    if (input.Skill == null) { return (null, "this metric needs a skill"); }
                    rule.Skill = input.Skill;
                    break;
                }
                case RoleMetric.CollectionLogItem:
                {
                    string item = (input.Item ?? "").Trim();
                    if (item.Length == 0) { return (null, "this metric needs an item"); }
                    int? itemId = int.TryParse(item, NumberStyles.None, CultureInfo.InvariantCulture, out int osrsId)
                        ? await db.Items.Where(x => x.OsrsId == osrsId).Select(x => (int?)x.Id).FirstOrDefaultAsync()
                        : await db.Items.Where(x => x.OsrsName.ToLower() == item.ToLower()).Select(x => (int?)x.Id).FirstOrDefaultAsync();
                    if (itemId == null) { return (null, $"item '{input.Item}' not found (run /syncitems first)"); }
                    rule.ItemId = itemId;
                    break;
                }
            }
            return (rule, null);
        }
    }
}
