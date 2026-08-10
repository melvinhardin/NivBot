using Microsoft.EntityFrameworkCore;
using NivBot.DataLayer;
using NivBot.DataLayer.Models;
using NivBot.ExternalServicesLayer.OsrsAPI;
using NivBot.ExternalServicesLayer.OsrsAPI.Models;


namespace NivBot.Features.LinkOsrsAccount
{
    public class LinkOsrsAccountService(IOsrsHighscoreService osrsApi, GoodplaceContext db)
    {
        public async Task<LinkOsrsAccountResult> LinkAccountAsync(long discId, string osrsname)
        {
            // Get all the needed information and check failure states, exit if fail

            // Check if the user is registered
            GoodplaceUser? user = await db.GoodplaceUsers
                .FirstOrDefaultAsync(x => x.DiscordUserId == discId);
            if (user == null) { return LinkOsrsAccountResult.FailureUserNotRegistered; }

            // Check if the API is up and the account exists
            PlayerStats? osrsAccount = await osrsApi.GetPlayerStatsAsync(osrsname);
            if (osrsAccount == null) { return LinkOsrsAccountResult.FailureNotOnHighscores; }

            // Check if the runescape account already exists in the database
            RunescapeAccount? accountExists = await db.RunescapeAccounts
                .FirstOrDefaultAsync(x => x.RunescapeName == osrsname);
            if (accountExists != null) { return LinkOsrsAccountResult.FailureOsrsNameAlreadyTaken; }

            // Create a dictionary of existing activities for insert
            var activitiesDict = await db.Activities
                .Select(x => new { x.OsrsName, x.Id })
                .ToDictionaryAsync(y => y.OsrsName, y => y.Id);

            // Create the query for adding a new osrs account
            RunescapeAccount runescapeAccount = new RunescapeAccount
            {
                RunescapeName = osrsname.ToLower(),
                GoodplaceUserId = user.Id,
                RunescapeStats = Enum
                    .GetValues<DataLayer.Enums.Skills>()
                    .Select(s => new RunescapeStat { Skill = s, Xp = (int)osrsAccount.Skills[(int)s].Xp })
                    .ToList<RunescapeStat>(),

                ActivityLogs = osrsAccount.Activities
                    .Select(a => new ActivityLog
                    {
                        ActivityId = activitiesDict[a.Name],
                        Amount = a.Score
                    })
                    .ToList<ActivityLog>()
            };
            db.RunescapeAccounts.Add(runescapeAccount);
            try
            {
                await db.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                return LinkOsrsAccountResult.FailureDatabaseSaveFailed;
            }
            return LinkOsrsAccountResult.SuccessAccountAdded;
        }
    }
}
