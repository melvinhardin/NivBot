using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NivBot.DataLayer;
using NivBot.DataLayer.Models;
using NivBot.ExternalServicesLayer.OsrsAPI;
using NivBot.Features.GoodplaceTask;
using NivBot.Features.Shared;
using System.Text.Json;
using PlayerStats = NivBot.ExternalServicesLayer.OsrsAPI.Models.PlayerStats;

namespace NivBot.Features.LinkOsrsAccount
{
    public class LinkOsrsAccountService(
        IOsrsHighscoreService osrsApi,
        GoodplaceContext db,
        GoodplaceTaskService taskService,
        ILogger<LinkOsrsAccountService> logger)
    {
        public async Task<LinkOsrsAccountResult> LinkAccountAsync(long discId, string osrsname)
        {
            osrsname = OsrsNames.Normalize(osrsname);

            // Check if the user is registered
            GoodplaceUser? user = await db.GoodplaceUsers
                .FirstOrDefaultAsync(x => x.DiscordUserId == discId);
            if (user == null) { return LinkOsrsAccountResult.FailureUserNotRegistered; }

            // Check if the runescape account is already linked (before calling the hiscores)
            bool accountExists = await db.RunescapeAccounts.AnyAsync(x => x.RunescapeName == osrsname);
            if (accountExists) { return LinkOsrsAccountResult.FailureOsrsNameAlreadyTaken; }

            // Check if the account exists on the hiscores
            PlayerStats? osrsAccount;
            try
            {
                osrsAccount = await osrsApi.GetPlayerStatsAsync(osrsname);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                logger.LogWarning(ex, "Hiscores lookup failed for {Account}", osrsname);
                return LinkOsrsAccountResult.FailureHiscoresUnavailable;
            }
            if (osrsAccount == null) { return LinkOsrsAccountResult.FailureNotOnHighscores; }

            RunescapeAccount runescapeAccount = new RunescapeAccount
            {
                RunescapeName = osrsname,
                GoodplaceUserId = user.Id,
                RunescapeStats = new List<RunescapeStat>(),
                ActivityLogs = new List<ActivityLog>()
            };
            OsrsAccountStatsService.ApplyStats(runescapeAccount, osrsAccount, await db.Activities.ToDictionaryAsync(x => x.OsrsName, x => x.Id));
            db.RunescapeAccounts.Add(runescapeAccount);

            // XP and kills this account already has must not complete a running task
            await taskService.ShiftTasksForNewAccountAsync(user.Id, runescapeAccount);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Most likely someone linked the same name at the same moment (unique index)
                logger.LogWarning(ex, "Saving linked account {Account} failed", osrsname);
                return LinkOsrsAccountResult.FailureDatabaseSaveFailed;
            }
            return LinkOsrsAccountResult.SuccessAccountAdded;
        }
    }
}
