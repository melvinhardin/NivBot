using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetCord.Rest;
using NivBot.DataLayer;
using NivBot.DataLayer.Enums;
using NivBot.DataLayer.Models;
using NivBot.ExternalServicesLayer;
using NivBot.ExternalServicesLayer.TempleAPI;
using NivBot.ExternalServicesLayer.WiseOldManAPI;
using NivBot.Features.Shared;
using System.Globalization;
using System.Text;

namespace NivBot.Features.Competitions
{
    public sealed record CompetitionView(
        int Id,
        APIProviders Provider,
        string ExternalId,
        string Title,
        string Metric,
        DateTime StartUtc,
        DateTime EndUtc,
        bool Finished)
    {
        public string Url => Provider == APIProviders.Wiseoldman
            ? $"https://wiseoldman.net/competitions/{ExternalId}"
            : $"https://templeosrs.com/competitions/standings.php?id={ExternalId}";
    }

    // Skill / boss competitions on Wise Old Man or TempleOSRS for the clan's group.
    // When one ends, the results are posted where it was created and the top places get points.
    public class CompetitionService(
        GoodplaceContext db,
        IWiseOldManService wiseOldMan,
        ITempleService temple,
        WiseOldManOptions womOptions,
        TempleOptions templeOptions,
        CompetitionOptions options,
        RestClient rest,
        ILogger<CompetitionService> logger)
    {
        // WOM limits titles to 50 characters
        private const int MaxTitleLength = 50;

        public async Task<(CompetitionView? Competition, string? Error)> CreateAsync(
            APIProviders provider, Skills? skill, string? boss, DateTime startUtc, DateTime endUtc, string? title, long guildId, long channelId)
        {
            if ((skill == null) == string.IsNullOrWhiteSpace(boss)) { return (null, "pick either a skill or a boss"); }
            if (endUtc <= startUtc) { return (null, "the end must be after the start"); }
            if (startUtc < DateTime.UtcNow.AddMinutes(1)) { return (null, "the start must be in the future"); }

            string metricName;
            string? womMetric, templeMetric;
            int? activityId = null;
            if (skill is Skills chosenSkill)
            {
                metricName = chosenSkill.ToString();
                womMetric = chosenSkill.ToString().ToLowerInvariant();
                // Temple calls Runecrafting "runecraft"
                templeMetric = chosenSkill == Skills.Runecrafting ? "runecraft" : chosenSkill.ToString().ToLowerInvariant();
            }
            else
            {
                string search = boss!.Trim();
                Boss? bossInfo = (await db.Bosses.AsNoTracking().ToListAsync())
                    .FirstOrDefault(x => x.OsrsName.Equals(search, StringComparison.OrdinalIgnoreCase));
                if (bossInfo == null) { return (null, $"'{search}' isn't a boss"); }

                string lower = bossInfo.OsrsName.ToLower();
                Activity? activity = await db.Activities.AsNoTracking().FirstOrDefaultAsync(x => x.OsrsName.ToLower() == lower);
                if (activity == null) { return (null, $"'{bossInfo.OsrsName}' isn't in the Activities table yet, run /syncactivities first"); }

                activityId = activity.Id;
                metricName = activity.OsrsName;
                womMetric = bossInfo.WomMetric;
                templeMetric = bossInfo.TempleMetric;
            }

            string finalTitle = string.IsNullOrWhiteSpace(title) ? $"{metricName} competition" : title.Trim();
            if (finalTitle.Length > MaxTitleLength) { finalTitle = finalTitle[..MaxTitleLength]; }

            string externalId, key;
            try
            {
                switch (provider)
                {
                    case APIProviders.Wiseoldman:
                        if (womOptions.GroupId == 0 || string.IsNullOrWhiteSpace(womOptions.GroupVerificationCode))
                        {
                            return (null, "set WiseOldMan:GroupId and WiseOldMan:GroupVerificationCode in the config first");
                        }
                        if (string.IsNullOrWhiteSpace(womMetric)) { return (null, $"{metricName} has no Wise Old Man metric"); }
                        (externalId, key) = await wiseOldMan.CreateCompetitionAsync(finalTitle, womMetric, startUtc, endUtc, womOptions.GroupId, womOptions.GroupVerificationCode);
                        break;
                    case APIProviders.Temple:
                        if (templeOptions.GroupId == 0 || string.IsNullOrWhiteSpace(templeOptions.GroupKey))
                        {
                            return (null, "set Temple:GroupId and Temple:GroupKey in the config first");
                        }
                        if (string.IsNullOrWhiteSpace(templeMetric)) { return (null, $"{metricName} has no TempleOSRS metric"); }
                        (externalId, key) = await temple.CreateCompetitionAsync(finalTitle, templeMetric, startUtc, endUtc, templeOptions.GroupId, templeOptions.GroupKey);
                        break;
                    default:
                        return (null, "unknown provider");
                }
            }
            catch (ExternalApiException ex)
            {
                return (null, ex.Message);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                logger.LogWarning(ex, "Creating a {Provider} competition failed", provider);
                return (null, $"{provider} can't be reached right now, try again later");
            }

            var details = new CompetitionProviderDetails
            {
                CompetitionProvider = provider,
                ExternalId = externalId,
                CompetitionKey = key,
                Title = finalTitle,
                GuildId = guildId,
                ChannelId = channelId
            };
            db.CompetitionProvidersDetails.Add(details);
            if (skill is Skills competitionSkill)
            {
                db.SkillCompetitions.Add(new SkillCompetition
                {
                    CompetitionProviderDetails = details,
                    Skill = competitionSkill,
                    StartTime = startUtc,
                    EndTime = endUtc
                });
            }
            else
            {
                db.ActivityCompetitions.Add(new ActivityCompetition
                {
                    CompetitionProviderDetails = details,
                    ActivityId = activityId!.Value,
                    StartTime = startUtc,
                    EndTime = endUtc
                });
            }
            await db.SaveChangesAsync();

            return (new CompetitionView(details.Id, provider, externalId, finalTitle, metricName, startUtc, endUtc, false), null);
        }

        // Newest first.
        public async Task<List<CompetitionView>> ListAsync(int count)
        {
            List<CompetitionView> all = await LoadAsync(onlyUnfinished: false);
            return all.OrderByDescending(x => x.EndUtc).Take(count).ToList();
        }

        // Deletes the competition at the provider and here. localOnly skips the provider (when it's already gone there).
        // Returns an error message, or null when deleted.
        public async Task<string?> DeleteAsync(int id, bool localOnly)
        {
            CompetitionProviderDetails? details = await db.CompetitionProvidersDetails.FirstOrDefaultAsync(x => x.Id == id);
            if (details == null) { return $"there is no competition {id}"; }

            if (!localOnly)
            {
                try
                {
                    if (details.CompetitionProvider == APIProviders.Wiseoldman)
                    {
                        await wiseOldMan.DeleteCompetitionAsync(details.ExternalId, details.CompetitionKey);
                    }
                    else
                    {
                        await temple.DeleteCompetitionAsync(details.ExternalId, details.CompetitionKey);
                    }
                }
                catch (ExternalApiException ex)
                {
                    return $"{ex.Message} (use local_only if it's already gone there)";
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
                {
                    return $"{details.CompetitionProvider} can't be reached right now, try again later";
                }
            }

            // The skill / activity competition rows go with it (cascade)
            db.CompetitionProvidersDetails.Remove(details);
            await db.SaveChangesAsync();
            return null;
        }

        // Asks WOM to update everyone in the last hour, so the results include the final gains.
        // requested: competitions already asked during this run of the bot.
        public async Task RequestFinalUpdatesAsync(ISet<int> requested, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.UtcNow;
            List<CompetitionView> endingSoon = (await LoadAsync(onlyUnfinished: true))
                .Where(x => x.Provider == APIProviders.Wiseoldman && x.EndUtc > now && x.EndUtc <= now.AddHours(1) && !requested.Contains(x.Id))
                .ToList();

            foreach (CompetitionView competition in endingSoon)
            {
                cancellationToken.ThrowIfCancellationRequested();
                requested.Add(competition.Id);
                string? key = await db.CompetitionProvidersDetails
                    .Where(x => x.Id == competition.Id)
                    .Select(x => x.CompetitionKey)
                    .FirstOrDefaultAsync(cancellationToken);
                if (key == null) { continue; }

                try
                {
                    await wiseOldMan.UpdateAllParticipantsAsync(competition.ExternalId, key);
                }
                catch (Exception ex) when (ex is ExternalApiException or HttpRequestException or TaskCanceledException)
                {
                    // e.g. nobody is outdated, nothing to do
                    logger.LogInformation("WOM update-all for competition {Id}: {Message}", competition.ExternalId, ex.Message);
                }
            }
        }

        // Posts the results of ended competitions and awards the top places.
        public async Task FinishEndedAsync(CancellationToken cancellationToken)
        {
            DateTime now = DateTime.UtcNow;
            List<CompetitionView> ended = (await LoadAsync(onlyUnfinished: true))
                .Where(x => x.EndUtc + options.ResultsDelay <= now)
                .ToList();

            foreach (CompetitionView competition in ended)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await FinishAsync(competition, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "Finishing competition {Id} failed, retrying later", competition.Id);
                }
                db.ChangeTracker.Clear();
            }
        }

        private async Task FinishAsync(CompetitionView competition, CancellationToken cancellationToken)
        {
            CompetitionProviderDetails details = await db.CompetitionProvidersDetails.FirstAsync(x => x.Id == competition.Id, cancellationToken);

            List<CompetitionStanding> standings;
            try
            {
                standings = competition.Provider == APIProviders.Wiseoldman
                    ? await wiseOldMan.GetCompetitionStandingsAsync(competition.ExternalId)
                    : await temple.GetCompetitionStandingsAsync(competition.ExternalId);
            }
            catch (Exception ex) when (ex is ExternalApiException or HttpRequestException or TaskCanceledException)
            {
                // Give up two days after the end (e.g. the competition was deleted on the website)
                if (competition.EndUtc.AddDays(2) > DateTime.UtcNow) { throw; }
                details.FinishedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
                await PostAsync(details.ChannelId, $"**{competition.Title}** has ended, but the results couldn't be read from {competition.Provider}: {competition.Url}");
                return;
            }

            int[] rewards = options.GetRewards();
            List<CompetitionStanding> winners = standings.Where(x => x.Gained > 0).Take(rewards.Length).ToList();

            // Linked accounts of the winners (names normalized like when linking)
            HashSet<string> names = winners.Select(x => OsrsNames.Normalize(x.Name)).ToHashSet();
            Dictionary<string, int> owners = await db.RunescapeAccounts
                .Where(x => names.Contains(x.RunescapeName))
                .ToDictionaryAsync(x => x.RunescapeName, x => x.GoodplaceUserId, cancellationToken);

            string unit = competition.Metric.Length > 0 && Enum.TryParse(competition.Metric, out Skills _) ? "XP" : "kills";
            var text = new StringBuilder($"**{competition.Title}** has ended!\n");
            for (int i = 0; i < winners.Count; i++)
            {
                CompetitionStanding winner = winners[i];
                text.Append(CultureInfo.InvariantCulture, $"{i + 1}. {winner.Name}: {winner.Gained:N0} {unit}");

                if (owners.TryGetValue(OsrsNames.Normalize(winner.Name), out int userId) && rewards[i] > 0)
                {
                    // Find also sees a wallet added earlier in this loop (two accounts of one member in the top)
                    Wallet? wallet = await db.Wallets.FindAsync(new object[] { userId }, cancellationToken);
                    if (wallet == null)
                    {
                        wallet = new Wallet { GoodplaceUserId = userId };
                        db.Wallets.Add(wallet);
                    }
                    wallet.GoodplacePoints += rewards[i];
                    wallet.GoodplaceCurrency += rewards[i];
                    text.Append(CultureInfo.InvariantCulture, $", +{rewards[i]} points");
                }
                else if (rewards[i] > 0)
                {
                    text.Append(" (not linked to the bot, no points)");
                }
                text.Append('\n');
            }
            if (winners.Count == 0) { text.Append("Nobody gained anything.\n"); }
            text.Append("Full results: ").Append(competition.Url);

            // Save first, so a failed post never awards the points twice
            details.FinishedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            await PostAsync(details.ChannelId, text.ToString());
        }

        private async Task PostAsync(long channelId, string content)
        {
            try
            {
                await rest.SendMessageAsync((ulong)channelId, new MessageProperties
                {
                    Content = content.Length > 2000 ? content[..2000] : content,
                    AllowedMentions = AllowedMentionsProperties.None
                });
            }
            catch (Exception ex) when (ex is RestException or HttpRequestException or TaskCanceledException)
            {
                logger.LogWarning(ex, "Posting competition results in channel {Channel} failed", channelId);
            }
        }

        private async Task<List<CompetitionView>> LoadAsync(bool onlyUnfinished)
        {
            var skills = await db.SkillCompetitions
                .AsNoTracking()
                .Where(x => !onlyUnfinished || x.CompetitionProviderDetails.FinishedAt == null)
                .Select(x => new
                {
                    x.CompetitionProviderDetails.Id,
                    x.CompetitionProviderDetails.CompetitionProvider,
                    x.CompetitionProviderDetails.ExternalId,
                    x.CompetitionProviderDetails.Title,
                    x.Skill,
                    x.StartTime,
                    x.EndTime,
                    x.CompetitionProviderDetails.FinishedAt
                })
                .ToListAsync();
            var activities = await db.ActivityCompetitions
                .AsNoTracking()
                .Where(x => !onlyUnfinished || x.CompetitionProviderDetails.FinishedAt == null)
                .Select(x => new
                {
                    x.CompetitionProviderDetails.Id,
                    x.CompetitionProviderDetails.CompetitionProvider,
                    x.CompetitionProviderDetails.ExternalId,
                    x.CompetitionProviderDetails.Title,
                    x.Activity.OsrsName,
                    x.StartTime,
                    x.EndTime,
                    x.CompetitionProviderDetails.FinishedAt
                })
                .ToListAsync();

            return skills
                .Select(x => new CompetitionView(x.Id, x.CompetitionProvider, x.ExternalId, x.Title, x.Skill.ToString(), x.StartTime, x.EndTime, x.FinishedAt != null))
                .Concat(activities.Select(x => new CompetitionView(x.Id, x.CompetitionProvider, x.ExternalId, x.Title, x.OsrsName, x.StartTime, x.EndTime, x.FinishedAt != null)))
                .ToList();
        }
    }
}
