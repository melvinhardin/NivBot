using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NivBot.Features.Shared;
using System.Globalization;
using System.Text;

namespace NivBot.Features.Points
{
    public class PointsModule(PointsService pointsService, ILogger<PointsModule> logger) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("points", "Show your Goodplace points and currency")]
        public async Task PointsAsync()
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            string reply;
            try
            {
                WalletView? wallet = await pointsService.GetWalletAsync((long)Context.User.Id);
                reply = wallet == null
                    ? "You are not registered yet, use /register first."
                    : $"You have **{Number(wallet.Points)}** points and **{Number(wallet.Currency)}** currency (rank #{wallet.Rank}).";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/points failed for {User}", Context.User.Id);
                reply = "Something went wrong, try again later.";
            }
            await ModifyResponseAsync(message => message.Content = reply);
        }

        [SlashCommand("leaderboard", "Show the top 10 by Goodplace points")]
        public async Task LeaderboardAsync()
        {
            await RespondAsync(InteractionCallback.DeferredMessage());

            string reply;
            try
            {
                List<LeaderboardEntry> entries = await pointsService.GetLeaderboardAsync(10);
                if (entries.Count == 0)
                {
                    reply = "Nobody has points yet.";
                }
                else
                {
                    var text = new StringBuilder("**Goodplace leaderboard**\n");
                    for (int i = 0; i < entries.Count; i++)
                    {
                        text.Append(CultureInfo.InvariantCulture, $"{i + 1}. <@{entries[i].DiscordUserId}>: {Number(entries[i].Points)} points\n");
                    }
                    reply = text.ToString();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/leaderboard failed");
                reply = "Something went wrong, try again later.";
            }

            // Mentions show the names without pinging anyone
            await ModifyResponseAsync(message =>
            {
                message.Content = reply;
                message.AllowedMentions = AllowedMentionsProperties.None;
            });
        }

        // Admin only, see SyncActivitiesModule
        [SlashCommand("setitempoints", "Set the points a collection log item gives",
            DefaultGuildPermissions = Permissions.Administrator,
            Contexts = new[] { InteractionContextType.Guild })]
        public async Task SetItemPointsAsync(
            [SlashCommandParameter(Name = "item", Description = "The collection log item", AutocompleteProviderType = typeof(ItemAutocompleteProvider))] string item,
            [SlashCommandParameter(Name = "points", Description = "Points (and currency) per item obtained", MinValue = 0, MaxValue = PointsService.MaxPoints)] int points)
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            string reply;
            try
            {
                string? name = await pointsService.SetItemPointsAsync(item, points);
                reply = name == null
                    ? "Item not found. Pick one from the list (run /syncitems first if the list is empty)."
                    : $"**{name}** now gives {points} points.";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/setitempoints failed");
                reply = "Something went wrong, try again later.";
            }
            await ModifyResponseAsync(message => message.Content = reply);
        }

        // Admin only, see SyncActivitiesModule
        [SlashCommand("setbosspoints", "Set the bonus points for completing a boss task of a boss",
            DefaultGuildPermissions = Permissions.Administrator,
            Contexts = new[] { InteractionContextType.Guild })]
        public async Task SetBossPointsAsync(
            [SlashCommandParameter(Name = "boss", Description = "The boss", AutocompleteProviderType = typeof(BossAutocompleteProvider))] string boss,
            [SlashCommandParameter(Name = "points", Description = "Bonus points (and currency) on top of the normal task reward", MinValue = 0, MaxValue = PointsService.MaxPoints)] int points)
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            string reply;
            try
            {
                string? name = await pointsService.SetBossPointsAsync(boss, points);
                reply = name == null
                    ? "Boss not found. Pick one from the list (run /syncactivities first if the list is empty)."
                    : $"Boss tasks for **{name}** now give {points} bonus points.";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/setbosspoints failed");
                reply = "Something went wrong, try again later.";
            }
            await ModifyResponseAsync(message => message.Content = reply);
        }

        private static string Number(long value) => value.ToString("N0", CultureInfo.InvariantCulture);
    }
}
