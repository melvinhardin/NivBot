using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NivBot.ExternalServicesLayer.TempleAPI;

namespace NivBot.Features.SyncUserCollections
{
    public class SyncOsrsAccountCollectionModule(
        SyncOsrsAccountCollectionService syncAccountItems,
        TempleOptions templeOptions,
        ILogger<SyncOsrsAccountCollectionModule> logger) : ApplicationCommandModule<ApplicationCommandContext>
    {
        // Admin only, see SyncActivitiesModule
        [SlashCommand("syncgroupitems", "Attempt to add all items from temple to collectionlogs",
            DefaultGuildPermissions = Permissions.Administrator,
            Contexts = new[] { InteractionContextType.Guild })]
        public async Task SyncAccountItems(
            [SlashCommandParameter(Name = "groupid", Description = "Id of the temple group, defaults to Temple:GroupId from the config")] int groupid = 0)
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            if (groupid == 0) { groupid = templeOptions.GroupId; }

            // Making sure whatever happens on the bot side, the user always gets a response.
            string reply;
            if (groupid == 0)
            {
                reply = "No group id given and Temple:GroupId isn't set in the config.";
            }
            else
            {
                try
                {
                    reply = await syncAccountItems.SyncGroupAccountCollog(groupid) switch
                    {
                        SyncOsrsAccountCollectionResult.Success =>
                            "Group collections were updated",
                        _ => "Failed to sync the group collections"
                    };
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "/syncgroupitems failed for group {GroupId}", groupid);
                    reply = "Something went wrong, try again later.";
                }
            }
            await ModifyResponseAsync(message => message.Content = reply);
        }

        [SlashCommand("synccollection", "Update the collection log of your linked accounts from TempleOSRS")]
        public async Task SyncOwnCollectionsAsync()
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            string reply;
            try
            {
                UserCollectionSyncResult result = await syncAccountItems.SyncUserCollectionsAsync((long)Context.User.Id);
                reply = result.Status switch
                {
                    SyncOsrsAccountCollectionResult.NotRegistered => "You are not registered yet, use /register first.",
                    SyncOsrsAccountCollectionResult.NoAccounts => "Link an Old School RuneScape account first with /addcharacter.",
                    SyncOsrsAccountCollectionResult.TempleUnavailable => "TempleOSRS can't be reached right now, try again in a few minutes.",
                    SyncOsrsAccountCollectionResult.Success => Summary(result),
                    _ => "Something went wrong, try again later."
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/synccollection failed for {User}", Context.User.Id);
                reply = "Something went wrong, try again later.";
            }
            await ModifyResponseAsync(message => message.Content = reply);
        }

        private static string Summary(UserCollectionSyncResult result)
        {
            string text = $"Synced {result.AccountsSynced} account{(result.AccountsSynced == 1 ? "" : "s")}, +{result.PointsAwarded} points.";
            if (result.AccountsNotOnTemple > 0)
            {
                text += $"\n{result.AccountsNotOnTemple} account{(result.AccountsNotOnTemple == 1 ? " has" : "s have")} no collection log on TempleOSRS: "
                    + "sync it in game with the TempleOSRS RuneLite plugin first.";
            }
            if (result.FirstSyncs > 0)
            {
                text += "\nThe first sync of an account only records what it already has, new items give points from the next sync on.";
            }
            return text;
        }
    }
}
