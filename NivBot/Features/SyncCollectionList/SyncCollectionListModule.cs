using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NivBot.Features.SyncActivities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.Features.SyncCollectionList
{
    public class SyncCollectionListModule(SyncCollectionListService syncItems) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("syncitems", "Attempt to add new items to the database")]
        public async Task SyncItems()
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(NetCord.MessageFlags.Ephemeral));

            // Making sure whatever happens on the bot side, the user always gets a response. 
            string reply;
            try
            {
                reply = await syncItems.SyncItemList() switch
                {
                    SyncCollectionListResult.FailureDbConnection =>
                        "Failed, could not connect to the database",
                    SyncCollectionListResult.FailureDbSave =>
                        "Failed saving to the database",
                    SyncCollectionListResult.FailureTempleApiConnection =>
                        "Failed, could not connect to the Temple API",
                    SyncCollectionListResult.SuccessSync =>
                        "New items were added to the database",
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
