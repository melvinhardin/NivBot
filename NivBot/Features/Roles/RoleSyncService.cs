using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NivBot.DataLayer;
using NivBot.DataLayer.Models;
using NivBot.ExternalServicesLayer.TempleAPI;
using NivBot.Features.Shared;
using NivBot.Features.SyncUserCollections;
using System.Globalization;
using System.Net;

namespace NivBot.Features.Roles
{
    // What a sync changed for one member.
    public sealed record RoleSyncSummary(List<string> Added, List<string> Removed, List<string> Errors);

    // Gives members the roles they qualify for (see DiscordRole). Never removes a role, except the lower tiers
    // of a tier group once a higher tier is given. Roles without a rule are never touched.
    public class RoleSyncService(
        GoodplaceContext db,
        OsrsAccountStatsService statsService,
        SyncOsrsAccountCollectionService collectionService,
        RestClient rest,
        RolesOptions options,
        TempleOptions templeOptions,
        ILogger<RoleSyncService> logger)
    {
        // The periodic sync: Temple group collection logs first (if a group is configured, this also awards
        // collection log points), then every member's roles.
        public async Task SyncAllAsync(CancellationToken cancellationToken)
        {
            if (templeOptions.GroupId != 0)
            {
                try
                {
                    await collectionService.SyncGroupAccountCollog(templeOptions.GroupId);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "Temple group collection sync before the role sync failed");
                }
                db.ChangeTracker.Clear();
            }

            if (!await db.DiscordRoles.AnyAsync(cancellationToken)) { return; }

            var members = await db.GoodplaceUsers
                .Where(x => x.RunescapeAccounts.Any())
                .Select(x => new { x.Id, x.DiscordUserId })
                .ToListAsync(cancellationToken);

            int changed = 0;
            foreach (var member in members)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    RoleSyncSummary summary = await SyncMemberAsync(member.Id, member.DiscordUserId, cancellationToken);
                    if (summary.Added.Count + summary.Removed.Count > 0) { changed++; }
                    foreach (string error in summary.Errors) { logger.LogWarning("Role sync for {User}: {Error}", member.DiscordUserId, error); }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "Role sync failed for {User}", member.DiscordUserId);
                }
                db.ChangeTracker.Clear();

                if (options.DelayBetweenMembers > TimeSpan.Zero) { await Task.Delay(options.DelayBetweenMembers, cancellationToken); }
            }
            logger.LogInformation("Role sync done: {Members} members checked, {Changed} changed", members.Count, changed);
        }

        // /updateroles: one member, right now. Null when the user isn't registered or has no linked accounts.
        public async Task<RoleSyncSummary?> SyncMemberAsync(long discordId, CancellationToken cancellationToken = default)
        {
            int? userId = await db.GoodplaceUsers
                .Where(x => x.DiscordUserId == discordId && x.RunescapeAccounts.Any())
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
            return userId == null ? null : await SyncMemberAsync(userId.Value, discordId, cancellationToken);
        }

        private async Task<RoleSyncSummary> SyncMemberAsync(int userId, long discordId, CancellationToken cancellationToken)
        {
            var summary = new RoleSyncSummary([], [], []);

            // Fresh stats from the hiscores. When they are down, the last known stats are used.
            var (refresh, accounts) = await statsService.RefreshUserAccountsAsync(userId);
            if (refresh == StatsRefreshResult.HiscoresUnavailable)
            {
                summary.Errors.Add("the hiscores couldn't be reached, used the last known stats");
            }
            await db.SaveChangesAsync(cancellationToken);

            List<DiscordRole> rules = await db.DiscordRoles
                .Include(x => x.Activity)
                .Include(x => x.Item)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            if (rules.Count == 0) { return summary; }

            MemberStats stats = await BuildStatsAsync(accounts, rules, cancellationToken);
            HashSet<long> noticed = (await db.RoleProgressNotices
                .Where(x => x.GoodplaceUserId == userId)
                .Select(x => x.DiscordRoleId)
                .ToListAsync(cancellationToken))
                .ToHashSet();

            foreach (IGrouping<long, DiscordRole> guildRules in rules.GroupBy(x => x.GuildId))
            {
                GuildUser member;
                try
                {
                    member = await rest.GetGuildUserAsync((ulong)guildRules.Key, (ulong)discordId, cancellationToken: cancellationToken);
                }
                catch (RestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    // Not (or no longer) in this server
                    continue;
                }
                catch (RestException ex)
                {
                    summary.Errors.Add($"can't read the member in server {guildRules.Key} ({(int)ex.StatusCode})");
                    continue;
                }

                HashSet<ulong> memberRoles = member.RoleIds.ToHashSet();
                (List<DiscordRole> give, List<DiscordRole> take) = DecideRoles(guildRules.ToList(), stats);

                foreach (DiscordRole rule in give.Where(x => !memberRoles.Contains((ulong)x.DiscordRoleId)))
                {
                    try
                    {
                        await rest.AddGuildUserRoleAsync((ulong)rule.GuildId, (ulong)discordId, (ulong)rule.DiscordRoleId, cancellationToken: cancellationToken);
                        memberRoles.Add((ulong)rule.DiscordRoleId);
                        summary.Added.Add(rule.Name);
                    }
                    catch (RestException ex)
                    {
                        summary.Errors.Add($"can't give {rule.Name} ({(int)ex.StatusCode}), check that the bot's role is above it and has Manage Roles");
                    }
                }

                foreach (DiscordRole rule in take.Where(x => memberRoles.Contains((ulong)x.DiscordRoleId)))
                {
                    try
                    {
                        await rest.RemoveGuildUserRoleAsync((ulong)rule.GuildId, (ulong)discordId, (ulong)rule.DiscordRoleId, cancellationToken: cancellationToken);
                        memberRoles.Remove((ulong)rule.DiscordRoleId);
                        summary.Removed.Add(rule.Name);
                    }
                    catch (RestException ex)
                    {
                        summary.Errors.Add($"can't remove {rule.Name} ({(int)ex.StatusCode})");
                    }
                }

                await PostCloseToNoticesAsync(userId, discordId, guildRules.ToList(), stats, memberRoles, noticed, cancellationToken);
            }

            await db.SaveChangesAsync(cancellationToken);
            return summary;
        }

        // Which roles the member should get, and which lower tiers to take away.
        private static (List<DiscordRole> Give, List<DiscordRole> Take) DecideRoles(List<DiscordRole> rules, MemberStats stats)
        {
            List<DiscordRole> qualified = rules.Where(x => stats.Value(x) >= x.Threshold).ToList();
            var give = new List<DiscordRole>();
            var take = new List<DiscordRole>();

            give.AddRange(qualified.Where(x => string.IsNullOrWhiteSpace(x.TierGroup)));

            foreach (IGrouping<string, DiscordRole> tier in qualified
                .Where(x => !string.IsNullOrWhiteSpace(x.TierGroup))
                .GroupBy(x => x.TierGroup!.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                DiscordRole highest = tier.OrderByDescending(x => x.Threshold).First();
                give.Add(highest);

                // Only lower tiers are removed: a higher tier given by hand stays
                take.AddRange(rules.Where(x =>
                    x.DiscordRoleId != highest.DiscordRoleId
                    && string.Equals(x.TierGroup?.Trim(), tier.Key, StringComparison.OrdinalIgnoreCase)
                    && x.Threshold < highest.Threshold));
            }
            return (give, take);
        }

        // Posts once per member and role when the member passes the role's close trigger.
        private async Task PostCloseToNoticesAsync(
            int userId,
            long discordId,
            List<DiscordRole> rules,
            MemberStats stats,
            HashSet<ulong> memberRoles,
            HashSet<long> noticed,
            CancellationToken cancellationToken)
        {
            if (options.AnnouncementChannelId == 0) { return; }

            foreach (DiscordRole rule in rules)
            {
                if (rule.CloseTrigger is not long trigger || noticed.Contains(rule.DiscordRoleId)) { continue; }
                if (memberRoles.Contains((ulong)rule.DiscordRoleId)) { continue; }

                long value = stats.Value(rule);
                if (value < trigger || value >= rule.Threshold) { continue; }

                // Not interesting when the member already has (or qualifies for) a higher tier of the same badge
                bool higherTier = !string.IsNullOrWhiteSpace(rule.TierGroup) && rules.Any(x =>
                    string.Equals(x.TierGroup?.Trim(), rule.TierGroup!.Trim(), StringComparison.OrdinalIgnoreCase)
                    && x.Threshold > rule.Threshold
                    && (memberRoles.Contains((ulong)x.DiscordRoleId) || stats.Value(x) >= x.Threshold));
                if (higherTier) { continue; }

                string emoji = string.IsNullOrWhiteSpace(rule.Emoji) ? "" : rule.Emoji.Trim() + " ";
                string content = $"{emoji}<@{discordId}> is close to **{rule.Name}**: "
                    + $"{value.ToString("N0", CultureInfo.InvariantCulture)} / {rule.Threshold.ToString("N0", CultureInfo.InvariantCulture)} {MemberStats.Describe(rule)}";
                try
                {
                    await rest.SendMessageAsync(options.AnnouncementChannelId, new MessageProperties
                    {
                        Content = content,
                        AllowedMentions = AllowedMentionsProperties.None
                    }, cancellationToken: cancellationToken);
                }
                catch (Exception ex) when (ex is RestException or HttpRequestException or TaskCanceledException)
                {
                    logger.LogWarning(ex, "Posting the close-to notice in channel {Channel} failed", options.AnnouncementChannelId);
                    return;
                }

                db.RoleProgressNotices.Add(new RoleProgressNotice
                {
                    GoodplaceUserId = userId,
                    DiscordRoleId = rule.DiscordRoleId,
                    NotifiedAt = DateTime.UtcNow
                });
                noticed.Add(rule.DiscordRoleId);
            }
        }

        private async Task<MemberStats> BuildStatsAsync(List<RunescapeAccount> accounts, List<DiscordRole> rules, CancellationToken cancellationToken)
        {
            // Activity id -> is a raid, for every activity that is a boss (matched by name like the boss tasks)
            Dictionary<string, bool> bosses = (await db.Bosses.AsNoTracking().ToListAsync(cancellationToken))
                .GroupBy(x => x.OsrsName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.First().IsRaid, StringComparer.OrdinalIgnoreCase);
            Dictionary<int, bool> bossActivities = (await db.Activities.AsNoTracking().ToListAsync(cancellationToken))
                .Where(x => bosses.ContainsKey(x.OsrsName))
                .ToDictionary(x => x.Id, x => bosses[x.OsrsName]);

            // Collection log amounts, only for the items rules ask about
            int[] itemIds = rules.Where(x => x.ItemId != null).Select(x => x.ItemId!.Value).Distinct().ToArray();
            int[] accountIds = accounts.Select(x => x.Id).ToArray();
            Dictionary<int, long> items = itemIds.Length == 0
                ? new Dictionary<int, long>()
                : (await db.CollectionLogs
                    .Where(x => accountIds.Contains(x.RunescapeAccountId) && itemIds.Contains(x.ItemId))
                    .Select(x => new { x.ItemId, x.Amount })
                    .ToListAsync(cancellationToken))
                    .GroupBy(x => x.ItemId)
                    .ToDictionary(x => x.Key, x => x.Sum(y => (long)y.Amount));

            return new MemberStats(accounts, bossActivities, items);
        }
    }
}
