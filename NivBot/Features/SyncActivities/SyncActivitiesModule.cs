using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace NivBot.Features.SyncActivities
{
    public class SyncActivitiesModule(SyncActivitiesService syncActivities, ILogger<SyncActivitiesModule> logger) : ApplicationCommandModule<ApplicationCommandContext>
    {
        // Admin only: hidden for members without the Administrator permission (server owners can change this
        // under Server Settings > Integrations) and not usable in DMs, where guild permissions don't apply.
        [SlashCommand("syncactivities", "Attempt to add new activities to the database",
            DefaultGuildPermissions = Permissions.Administrator,
            Contexts = new[] { InteractionContextType.Guild })]
        public async Task SyncActivities()
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            // Making sure whatever happens on the bot side, the user always gets a response.
            string reply;
            try
            {
                reply = await syncActivities.SyncActivities() switch
                {
                    SyncActivitiesResult.FailureDbConnection =>
                        "Failed, could not connect to the database",
                    SyncActivitiesResult.FailureDbSave =>
                        "Failed saving to the database",
                    SyncActivitiesResult.FailureNameChange =>
                        "Failed, an activity was renamed or removed on the hiscores. Fix the Activities table by hand.",
                    SyncActivitiesResult.FailureOsrsApiConnection =>
                        "Failed, could not connect to the OSRS hiscores",
                    SyncActivitiesResult.FailureReferencePlayerNotFound =>
                        "Failed, the reference player (Osrs:ReferencePlayer in the config) is not on the hiscores",
                    SyncActivitiesResult.SuccessNoChanges =>
                        "Success, no changes were found",
                    SyncActivitiesResult.Success =>
                        "New activities were added to the database",
                    _ => "Something terrible happened!"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/syncactivities failed");
                reply = "Something went wrong, try again later.";
            }
            await ModifyResponseAsync(message => message.Content = reply);
        }
    }
}
