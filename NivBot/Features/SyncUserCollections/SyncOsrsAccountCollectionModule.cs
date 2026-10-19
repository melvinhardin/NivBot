using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace NivBot.Features.SyncUserCollections
{
    public class SyncOsrsAccountCollectionModule(SyncOsrsAccountCollectionService syncAccountItems, ILogger<SyncOsrsAccountCollectionModule> logger) : ApplicationCommandModule<ApplicationCommandContext>
    {
        // Admin only, see SyncActivitiesModule
        [SlashCommand("syncgroupitems", "Attempt to add all items from temple to collectionlogs",
            DefaultGuildPermissions = Permissions.Administrator,
            Contexts = new[] { InteractionContextType.Guild })]
        public async Task SyncAccountItems([SlashCommandParameter(Name = "groupid", Description = "Id of the temple group to sync items from")] int groupid)
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            // Making sure whatever happens on the bot side, the user always gets a response.
            string reply;
            try
            {
                reply = await syncAccountItems.SyncGroupAccountCollog(groupid) switch
                {
                    SyncOsrsAccountCollectionResult.Failure =>
                        "Failed to sync the group collections",
                    SyncOsrsAccountCollectionResult.Success =>
                        "Group collections were updated",
                    _ => "Something terrible happened!"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/syncgroupitems failed for group {GroupId}", groupid);
                reply = "Something went wrong, try again later.";
            }
            await ModifyResponseAsync(message => message.Content = reply);
        }
    }
}
