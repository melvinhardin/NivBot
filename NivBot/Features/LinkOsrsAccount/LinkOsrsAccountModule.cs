using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace NivBot.Features.LinkOsrsAccount
{
    public class LinkOsrsAccountModule(LinkOsrsAccountService addOsrsAcc, ILogger<LinkOsrsAccountModule> logger) : ApplicationCommandModule<ApplicationCommandContext>
    {
        // Check if the user is registered and the account isn't linked yet, check the account exists on the hiscores,
        // then add the osrs account to the Db
        [SlashCommand("addcharacter", "Attempt to link an Old School Runescape account to your Discord Account.")]
        public async Task AddOsrsAccountAsync(
            [SlashCommandParameter(Name = "osrsname", Description = "Your Old School Runescape character name", MinLength = 1, MaxLength = 12)] string osrsname)
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            // Making sure whatever happens on the bot side, the user always gets a response.
            string reply;
            try
            {
                reply = await addOsrsAcc.LinkAccountAsync((long)Context.User.Id, osrsname) switch
                {
                    LinkOsrsAccountResult.FailureNotOnHighscores =>
                        "Account not found on the hiscores. Check the spelling, accounts with low levels may not be listed.",
                    LinkOsrsAccountResult.FailureHiscoresUnavailable =>
                        "The OSRS hiscores can't be reached right now, try again in a few minutes.",
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
                logger.LogError(ex, "/addcharacter failed for {User}", Context.User.Id);
                reply = "Something went wrong, try again later.";
            }

            await ModifyResponseAsync(message => message.Content = reply);
        }
    }
}
