using Microsoft.EntityFrameworkCore;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NivBot.DataLayer;
using NivBot.DataLayer.Models;
using NivBot.ExternalServicesLayer.OsrsAPI;
using NivBot.ExternalServicesLayer.OsrsAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;
using NivBot.DataLayer.Enums;
using System.ComponentModel.DataAnnotations;

namespace NivBot.Features.LinkOsrsAccount
{
    public class LinkOsrsAccountModule(LinkOsrsAccountService addOsrsAcc) : ApplicationCommandModule<ApplicationCommandContext>
    {

        // Check if the user is already in the Db, and if the account doesn't already exists in the db, lastly check if the account exists on the highscores
        // Then add specified osrs account to the Db
        [SlashCommand("addcharacter", "Attempt to link an Old School Runescape account to your Discord Account.")]
        public async Task AddOsrsAccountAsync([SlashCommandParameter(Name = "osrsname", Description = "Your Old School Runescape character name"), MaxLength(12)] string osrsname)
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(NetCord.MessageFlags.Ephemeral));
            
            // Making sure whatever happens on the bot side, the user always gets a response. 
            string reply;
            try
            {
                reply = await addOsrsAcc.LinkAccountAsync((long)Context.User.Id, osrsname) switch
                {
                    LinkOsrsAccountResult.FailureNotOnHighscores =>
                        "Account not found on highscores",
                    LinkOsrsAccountResult.FailureOsrsNameAlreadyTaken =>
                        "Account already linked to a user",
                    LinkOsrsAccountResult.FailureUserNotRegistered =>
                        "You are not registered, please register with /register",
                    LinkOsrsAccountResult.SuccessAccountAdded =>
                        $"Your account {osrsname} has been linked.",
                    LinkOsrsAccountResult.FailureDatabaseSaveFailed =>
                        "Something went wrong during saving, try again or contact an admin if this error reoccurs.",
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
