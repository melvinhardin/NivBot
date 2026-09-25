using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NivBot.Features.SyncCollectionList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NivBot.Features.SyncUserCollections
{
    public class SyncOsrsAccountCollectionModulepublic (SyncOsrsAccountCollectionService syncAccountItems) : ApplicationCommandModule<ApplicationCommandContext>
    
    {
        [SlashCommand("syncGroupItems", "Attempt to add all items from temple to collectionlogs")]
        public async Task SyncAccountItems([SlashCommandParameter(Name = "groupid", Description = "Id of the temple group to sync items from")] int groupid)
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(NetCord.MessageFlags.Ephemeral));

            // Making sure whatever happens on the bot side, the user always gets a response. 
            string reply;
            try
            {
                reply = await syncAccountItems.SyncGroupAccountCollog(groupid) switch
                {
                    SyncOsrsAccountCollectionResult.Failure =>
                        "Failed, could not connect to the database",
                    SyncOsrsAccountCollectionResult.Success =>
                        "Group collections were updated",
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
