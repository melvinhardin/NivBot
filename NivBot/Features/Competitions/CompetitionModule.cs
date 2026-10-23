using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NivBot.DataLayer.Enums;
using NivBot.Features.Shared;
using System.Globalization;
using System.Text;

namespace NivBot.Features.Competitions
{
    // Admin only: hidden for members without the Administrator permission and not usable in DMs.
    [SlashCommand("competition", "Manage skill and boss competitions",
        DefaultGuildPermissions = Permissions.Administrator,
        Contexts = new[] { InteractionContextType.Guild })]
    public class CompetitionModule(CompetitionService competitions, ILogger<CompetitionModule> logger) : ApplicationCommandModule<ApplicationCommandContext>
    {
        private static readonly string[] DateFormats = ["yyyy-MM-dd HH:mm", "yyyy-MM-dd H:mm", "yyyy-MM-dd"];

        [SubSlashCommand("create", "Create a competition for the clan; results are posted in this channel")]
        public async Task CreateAsync(
            [SlashCommandParameter(Name = "provider", Description = "Where the competition is tracked")] APIProviders provider,
            [SlashCommandParameter(Name = "skill", Description = "Skill of the competition (or pick a boss)")] Skills? skill = null,
            [SlashCommandParameter(Name = "boss", Description = "Boss of the competition (or pick a skill)", AutocompleteProviderType = typeof(BossAutocompleteProvider))] string? boss = null,
            [SlashCommandParameter(Name = "start", Description = "Start in UTC as yyyy-MM-dd HH:mm, default in 10 minutes")] string? start = null,
            [SlashCommandParameter(Name = "days", Description = "How many days it runs", MinValue = 1, MaxValue = 31)] int days = 7,
            [SlashCommandParameter(Name = "title", Description = "Title, defaults to '<skill or boss> competition'", MaxLength = 50)] string? title = null)
        {
            // Public, so the clan sees the new competition
            await RespondAsync(InteractionCallback.DeferredMessage());

            string reply;
            try
            {
                DateTime startUtc;
                if (string.IsNullOrWhiteSpace(start))
                {
                    DateTime now = DateTime.UtcNow.AddMinutes(10);
                    startUtc = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc);
                }
                else if (!DateTime.TryParseExact(start.Trim(), DateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out startUtc))
                {
                    await ModifyResponseAsync(message => message.Content = $"'{start}' isn't a date like 2026-11-01 18:00 (UTC).");
                    return;
                }

                var (competition, error) = await competitions.CreateAsync(
                    provider, skill, boss, startUtc, startUtc.AddDays(days), title,
                    (long)(Context.Interaction.GuildId ?? 0), (long)Context.Interaction.Channel.Id);

                reply = competition == null
                    ? $"Couldn't create the competition: {error}."
                    : $"**{competition.Title}** ({competition.Metric}) on {provider}: <t:{ToUnix(competition.StartUtc)}:f> to <t:{ToUnix(competition.EndUtc)}:f>.\n"
                      + $"{competition.Url}\nThe results and points are posted here when it ends (id {competition.Id}).";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/competition create failed");
                reply = "Something went wrong, try again later.";
            }
            await ModifyResponseAsync(message => message.Content = reply);
        }

        [SubSlashCommand("list", "Show the latest competitions")]
        public async Task ListAsync()
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            string reply;
            try
            {
                List<CompetitionView> list = await competitions.ListAsync(15);
                if (list.Count == 0)
                {
                    reply = "No competitions yet, create one with /competition create.";
                }
                else
                {
                    var text = new StringBuilder();
                    foreach (CompetitionView competition in list)
                    {
                        string state = competition.Finished ? "finished"
                            : competition.StartUtc > DateTime.UtcNow ? "upcoming"
                            : "running";
                        text.Append(CultureInfo.InvariantCulture,
                            $"`{competition.Id}` **{competition.Title}** ({competition.Metric}, {competition.Provider}, {state}): <t:{ToUnix(competition.StartUtc)}:d> to <t:{ToUnix(competition.EndUtc)}:d> <{competition.Url}>\n");
                    }
                    reply = text.ToString();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/competition list failed");
                reply = "Something went wrong, try again later.";
            }
            await ModifyResponseAsync(message => message.Content = reply.Length > 2000 ? reply[..2000] : reply);
        }

        [SubSlashCommand("delete", "Delete a competition at the provider and here")]
        public async Task DeleteAsync(
            [SlashCommandParameter(Name = "id", Description = "The id from /competition list")] int id,
            [SlashCommandParameter(Name = "local_only", Description = "Only forget it here, e.g. when it was already deleted on the website")] bool localOnly = false)
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            string reply;
            try
            {
                string? error = await competitions.DeleteAsync(id, localOnly);
                reply = error == null ? $"Competition {id} deleted." : $"Not deleted: {error}.";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/competition delete failed");
                reply = "Something went wrong, try again later.";
            }
            await ModifyResponseAsync(message => message.Content = reply);
        }

        private static long ToUnix(DateTime utc) => new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)).ToUnixTimeSeconds();
    }
}
