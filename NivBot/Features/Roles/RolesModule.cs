using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NivBot.DataLayer.Enums;
using NivBot.DataLayer.Models;
using NivBot.Features.Shared;
using System.Globalization;
using System.Text;

namespace NivBot.Features.Roles
{
    // Admin only: hidden for members without the Administrator permission and not usable in DMs.
    [SlashCommand("roles", "Manage the roles the bot hands out",
        DefaultGuildPermissions = Permissions.Administrator,
        Contexts = new[] { InteractionContextType.Guild })]
    public class RolesModule(
        RoleRulesService rulesService,
        RoleSyncTrigger syncTrigger,
        RestClient rest,
        IHttpClientFactory httpClientFactory,
        ILogger<RolesModule> logger) : ApplicationCommandModule<ApplicationCommandContext>
    {
        private const int MaxImportBytes = 256 * 1024;

        [SubSlashCommand("add", "Add or change the role members get when they reach a threshold")]
        public async Task AddAsync(
            [SlashCommandParameter(Name = "role", Description = "The Discord role")] Role role,
            [SlashCommandParameter(Name = "metric", Description = "What is measured")] RoleMetric metric,
            [SlashCommandParameter(Name = "threshold", Description = "Value needed for the role: KC, level, XP or count", MinValue = 0)] long threshold,
            [SlashCommandParameter(Name = "boss", Description = "Boss / activity, for ActivityScore", AutocompleteProviderType = typeof(ActivityAutocompleteProvider))] string? boss = null,
            [SlashCommandParameter(Name = "skill", Description = "Skill, for SkillLevel")] Skills? skill = null,
            [SlashCommandParameter(Name = "item", Description = "Collection log item, for CollectionLogItem", AutocompleteProviderType = typeof(ItemAutocompleteProvider))] string? item = null,
            [SlashCommandParameter(Name = "close_trigger", Description = "Post once when a member passes this value", MinValue = 0)] long? closeTrigger = null,
            [SlashCommandParameter(Name = "tier_group", Description = "Roles in the same tier group are tiers: members keep only the highest", MaxLength = 50)] string? tierGroup = null,
            [SlashCommandParameter(Name = "emoji", Description = "Emoji for posts, e.g. <:CollectorBadge:926004335014457456>", MaxLength = 100)] string? emoji = null,
            [SlashCommandParameter(Name = "name", Description = "Name in lists and posts, defaults to the role's name", MaxLength = 100)] string? name = null)
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            string reply;
            try
            {
                var input = new RoleRuleInput((long)role.Id, name ?? role.Name, metric, threshold, boss, skill, item, closeTrigger, tierGroup, emoji);
                string? error = await rulesService.SaveAsync((long)GuildId, input);
                reply = error == null
                    ? $"Saved: <@&{role.Id}> for {metric} {threshold.ToString("N0", CultureInfo.InvariantCulture)}. Members get it on the next sync (/roles sync to run it now)."
                    : $"Not saved: {error}.";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/roles add failed");
                reply = "Something went wrong, try again later.";
            }
            await ReplyAsync(reply);
        }

        [SubSlashCommand("remove", "Stop handing out a role (members keep it)")]
        public async Task RemoveAsync([SlashCommandParameter(Name = "role", Description = "The Discord role")] Role role)
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            string reply;
            try
            {
                reply = await rulesService.RemoveAsync((long)GuildId, (long)role.Id)
                    ? $"<@&{role.Id}> is no longer handed out. Members who have it keep it."
                    : $"<@&{role.Id}> has no rule.";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/roles remove failed");
                reply = "Something went wrong, try again later.";
            }
            await ReplyAsync(reply);
        }

        [SubSlashCommand("list", "Show the roles the bot hands out")]
        public async Task ListAsync()
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            List<string> lines;
            try
            {
                List<DiscordRole> rules = await rulesService.ListAsync((long)GuildId);
                lines = rules.Count == 0
                    ? ["No roles configured yet. Use /roles add or /roles import."]
                    : rules.Select(Describe).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/roles list failed");
                lines = ["Something went wrong, try again later."];
            }

            // Discord messages are limited to 2000 characters, send the list in parts
            List<string> parts = Split(lines, 1900);
            await ReplyAsync(parts[0]);
            foreach (string part in parts.Skip(1))
            {
                await FollowupAsync(new InteractionMessageProperties
                {
                    Content = part,
                    Flags = MessageFlags.Ephemeral,
                    AllowedMentions = AllowedMentionsProperties.None
                });
            }
        }

        [SubSlashCommand("import", "Add or change many roles at once from a CSV file (see roles-import.csv in the repository)")]
        public async Task ImportAsync([SlashCommandParameter(Name = "file", Description = "CSV with the columns " + RoleRulesService.ImportHeader)] Attachment file)
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            string reply;
            try
            {
                if (file.Size > MaxImportBytes)
                {
                    reply = "That file is too big for a role list.";
                }
                else
                {
                    string csv = await httpClientFactory.CreateClient().GetStringAsync(file.Url);
                    Dictionary<long, string> serverRoles = (await rest.GetGuildRolesAsync(GuildId))
                        .ToDictionary(x => (long)x.Id, x => x.Name);

                    RoleImportResult result = await rulesService.ImportAsync((long)GuildId, csv, serverRoles);
                    var text = new StringBuilder($"Saved {result.Saved} role rules.");
                    if (result.Problems.Count > 0)
                    {
                        text.Append(CultureInfo.InvariantCulture, $" {result.Problems.Count} rows weren't saved:");
                        foreach (string problem in result.Problems.Take(20)) { text.Append("\n- ").Append(problem); }
                        if (result.Problems.Count > 20) { text.Append(CultureInfo.InvariantCulture, $"\n- and {result.Problems.Count - 20} more"); }
                    }
                    reply = text.ToString();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/roles import failed");
                reply = "Something went wrong reading the file, check that it's a CSV with the columns " + RoleRulesService.ImportHeader;
            }
            await ReplyAsync(reply.Length > 1990 ? reply[..1990] : reply);
        }

        [SubSlashCommand("sync", "Check every member's roles now instead of waiting for the next sync")]
        public async Task SyncAsync()
        {
            syncTrigger.Request();
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties
            {
                Content = "Role sync started. It checks every member against the hiscores, so it can take a few minutes.",
                Flags = MessageFlags.Ephemeral
            }));
        }

        private ulong GuildId => Context.Interaction.GuildId
            ?? throw new InvalidOperationException("/roles can only be used in a server.");

        private Task ReplyAsync(string content) => ModifyResponseAsync(message =>
        {
            message.Content = content;
            message.AllowedMentions = AllowedMentionsProperties.None;
        });

        private static string Describe(DiscordRole rule)
        {
            var text = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(rule.Emoji)) { text.Append(rule.Emoji).Append(' '); }
            text.Append(CultureInfo.InvariantCulture, $"<@&{rule.DiscordRoleId}> **{rule.Name}**: {rule.Threshold:N0} {MemberStats.Describe(rule)}");
            if (rule.CloseTrigger is long close) { text.Append(CultureInfo.InvariantCulture, $", close at {close:N0}"); }
            if (!string.IsNullOrWhiteSpace(rule.TierGroup)) { text.Append(", tier group ").Append(rule.TierGroup); }
            return text.ToString();
        }

        private static List<string> Split(List<string> lines, int maxLength)
        {
            var parts = new List<string>();
            var current = new StringBuilder();
            foreach (string line in lines)
            {
                if (current.Length > 0 && current.Length + line.Length + 1 > maxLength)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                }
                if (current.Length > 0) { current.Append('\n'); }
                current.Append(line.Length > maxLength ? line[..maxLength] : line);
            }
            if (current.Length > 0 || parts.Count == 0) { parts.Add(current.ToString()); }
            return parts;
        }
    }

    public class UpdateRolesModule(RoleSyncService roleSync, ILogger<UpdateRolesModule> logger) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("updateroles", "Check your achievements against the hiscores and update your roles now",
            Contexts = new[] { InteractionContextType.Guild })]
        public async Task UpdateRolesAsync()
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            string reply;
            try
            {
                RoleSyncSummary? summary = await roleSync.SyncMemberAsync((long)Context.User.Id);
                if (summary == null)
                {
                    reply = "Register with /register and link an account with /addcharacter first.";
                }
                else
                {
                    var text = new StringBuilder();
                    if (summary.Added.Count > 0) { text.Append("New roles: ").Append(string.Join(", ", summary.Added)).Append('\n'); }
                    if (summary.Removed.Count > 0) { text.Append("Replaced lower tiers: ").Append(string.Join(", ", summary.Removed)).Append('\n'); }
                    if (text.Length == 0) { text.Append("Your roles are up to date.\n"); }
                    if (summary.Errors.Count > 0) { text.Append("Some roles couldn't be changed, tell an admin: ").Append(string.Join("; ", summary.Errors)); }
                    reply = text.ToString().TrimEnd();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/updateroles failed for {User}", Context.User.Id);
                reply = "Something went wrong, try again later.";
            }
            await ModifyResponseAsync(message => message.Content = reply.Length > 1990 ? reply[..1990] : reply);
        }
    }
}
