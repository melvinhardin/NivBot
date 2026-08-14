using Microsoft.EntityFrameworkCore;
using NetCord;
using NivBot.DataLayer;
using NivBot.DataLayer.Models;
using NivBot.ExternalServicesLayer.TempleAPI;
using NivBot.ExternalServicesLayer.TempleAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.Features.SyncUserCollections
{
    public class SyncOsrsAccountCollectionService(GoodplaceContext db, ITempleService templeApi)
    {
        public async Task<SyncOsrsAccountCollectionResult> SyncOsrsAccountCollog(string osrsAccount)
        {
            var onlineAccountCollog = await templeApi.GetAccountCollection(osrsAccount);

            // check if account is in the database
            RunescapeAccount account;
            try { account = await db.RunescapeAccounts.Where(x => x.RunescapeName == osrsAccount).FirstAsync(); }
            catch (ArgumentNullException) { return SyncOsrsAccountCollectionResult.FailureNotImplemented; }

            List<CollectionLog> dbCollogList;
            // Either make an empty list if the user doesnt have a collog, or populate it with existing data 
            if (!account.syncedColLog) { dbCollogList = new(); }
            else { dbCollogList = account.CollectionLogs.ToList(); }

            
            

            
            return SyncOsrsAccountCollectionResult.FailureNotImplemented;
        }
        public async Task<SyncOsrsAccountCollectionResult> SyncGroupAccountCollog(int groupId)
        {
            // Get the list from the API
            List<ParsedMember> groupCollog = await templeApi.GetGroupCollectionsAsync(groupId);

            // Make a hashset from the names to use in the db query
            HashSet<string> names = groupCollog
                .Select(x => x.OsrsName.ToLowerInvariant())
                .ToHashSet();

            // Get the relevant names from the db
            Dictionary<string, RunescapeAccount> accounts = await db.RunescapeAccounts
                .Where(x => names.Contains(x.RunescapeName))
                .ToDictionaryAsync(x => x.RunescapeName);
            
            // Get the internal account id's
            int[] accountIds = accounts.Values.Select(x => x.Id).ToArray();

            // Get all userId's of accounts
            int[] userIds = accounts.Values.Select(x => x.GoodplaceUserId).Distinct().ToArray();

            // Get the wallets linked on accountId's
            Dictionary<int, Wallet> wallets = await db.Wallets
                .Where(x => userIds.Contains(x.GoodplaceUserId))
                .ToDictionaryAsync(y => y.GoodplaceUserId);


            // Get the internal item id's and points, here item1 is the id, item2 is the points.
            Dictionary<int, (int, int)> itemIds = await db.Items.ToDictionaryAsync(x => x.OsrsId, x =>( x.Id, x.Points));

            // Query for the existing collectionlogs
            Dictionary<(int, int), CollectionLog> existingLogs = await db.CollectionLogs
                .Where(x => accountIds.Contains(x.RunescapeAccountId))
                .ToDictionaryAsync(y => (y.RunescapeAccountId, y.ItemId));

            foreach (var osrsCharacter in groupCollog)
            {
                // Skip to the next character if not in the db
                if (!accounts.TryGetValue(osrsCharacter.OsrsName.ToLowerInvariant(), out var account))
                { continue; }
                foreach (var item in osrsCharacter.Items)
                {
                    // Skip to the next item if not in the db
                    if (!itemIds.TryGetValue(item.OsrsId, out var itemId))
                    {
                        continue;
                    }

                    // Make sure to get item 1, this is the id
                    if (existingLogs.TryGetValue((account.Id, itemId.Item1), out var log))
                    {
                        if (log.Amount < item.Amount)
                        {
                            // Getting Items2 are the points assigned to the collog
                            // We can safely take the difference between the two because we already check if item.Amount is higher
                            if (account.syncedColLog) { wallets[account.GoodplaceUserId].GoodplacePoints += (itemId.Item2 * (item.Amount - log.Amount)); }
                            if (account.syncedColLog) { wallets[account.GoodplaceUserId].GoodplaceCurrency += (itemId.Item2 * (item.Amount - log.Amount)); }
                            // Make sure we set the log amount after so we can get the difference before
                            log.Amount = item.Amount;
                        }
                    }
                    else
                    {
                        var newCollectionLog = new CollectionLog
                        {
                            Amount = item.Amount,
                            ItemId = itemId.Item1,
                            RunescapeAccountId = account.Id
                        };
                        db.CollectionLogs.Add(newCollectionLog);
                        // Award points and currency
                        if (account.syncedColLog) { wallets[account.GoodplaceUserId].GoodplacePoints += (itemId.Item2 * item.Amount); }
                        if (account.syncedColLog) { wallets[account.GoodplaceUserId].GoodplaceCurrency += (itemId.Item2 * item.Amount); }
                        existingLogs[(account.Id, itemId.Item1)] = newCollectionLog;
                    }
                }
                // When we're done adding collectionlogs set the collog flag to synced if it wasn't
                if (!account.syncedColLog) { account.syncedColLog = true; }

                
            }
            await db.SaveChangesAsync();
            return SyncOsrsAccountCollectionResult.Success;

        }
    }
}
