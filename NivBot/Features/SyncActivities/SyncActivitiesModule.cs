using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NivBot.Features.RegisterGoodplaceUser;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.Features.SyncActivities
{
    public class SyncActivitiesModule(SyncActivitiesService syncActivities) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("syncactivities", "Attempt to add new activities to the database")]
        public async Task SyncActivities()
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(NetCord.MessageFlags.Ephemeral));

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
                        "Failed, a name has been changed, contact Melvin",
                    SyncActivitiesResult.FailureOsrsApiConnection =>
                        "Failed, could not connect to the Temple API",
                    SyncActivitiesResult.SuccessNoChanges =>
                        "Success, no changes were found",
                    SyncActivitiesResult.Success =>
                        "New activities were added to the database",
                    _ => "Something terrible happened!"
                };
            }
            catch (Exception ex)
            {
                reply = "Something went wrong, try again later.";
            }
            await Context.Interaction.ModifyResponseAsync(message => message.WithContent(reply));
        }
    }
}
