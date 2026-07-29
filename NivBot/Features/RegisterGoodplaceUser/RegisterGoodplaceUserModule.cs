using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NivBot.Features.LinkOsrsAccount;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NivBot.Features.RegisterGoodplaceUser
{
    public class RegisterGoodplaceUserModule(RegisterGoodplaceUserService registerUserService) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("register", "Attempt to register yourself to the bot")]
        public async Task AddGoodplaceUserAsync()
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(NetCord.MessageFlags.Ephemeral));

            // Making sure whatever happens on the bot side, the user always gets a response. 
            string reply;
            try
            {
                reply = await registerUserService.RegisterGoodplaceUser((long)Context.User.Id) switch
                {
                    RegisterGoodplaceUserResult.FailureUserAlreadyExists =>
                        "You are already registered!",
                    RegisterGoodplaceUserResult.FailureSavingToDb =>
                        "The database is on fire!",
                    RegisterGoodplaceUserResult.Success =>
                        "You are registered, add your runescape accounts with /addcharacter",
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
