using Microsoft.EntityFrameworkCore;
using NetCord;
using NivBot.DataLayer;
using NivBot.DataLayer.Models;
using NivBot.ExternalServicesLayer.TempleAPI;
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
            var groupCollog = await templeApi.GetGroupCollectionsAsync(groupId);

            // Get the lists from the db
            var dbCollogs = await db.CollectionLogs.ToListAsync();
            var dbItems = await db.Items.ToListAsync();
            var dbAccounts = await db.RunescapeAccounts.ToListAsync();
            foreach (var osrsCharacter in groupCollog)
            {
                RunescapeAccount? currentId = dbAccounts.Find(x => x.RunescapeName.Equals(osrsCharacter.OsrsName.ToLower()));
                Console.WriteLine(currentId);
                Console.WriteLine(osrsCharacter.OsrsName);
                // Continue to next iteration if character doesn't exist
                if(currentId is null) { continue; }


                // Make a list to capture any new collogs
                List<CollectionLog> newItems = new();

                foreach(var item in osrsCharacter.Items)
                {
                    // Go to next item if it doesnt exist in the db TODO add a way to call on the item sync funtion maybe?
                    if(!dbItems.Any(y => y.OsrsId == item.OsrsId))
                    { continue; }

                    // If the character doesnt have the item add it to the new item list.
                    if (
                    !dbCollogs
                        .Where(y => y.RunescapeAccountId == currentId.Id)
                        .Any(x => x.Item.OsrsId == item.OsrsId)
                        )
                    {
                        newItems.Add(
                            new CollectionLog
                            {
                                Amount = item.Amount,
                                ItemId = dbItems.First(y => y.OsrsId == item.OsrsId).Id,
                                RunescapeAccountId = currentId.Id
                            }
                        );
                        continue;
                    }

                    // Set the amount of the item
                    db.CollectionLogs
                        .Where(y => y.Item.OsrsId == item.OsrsId)
                        .Where(z => z.RunescapeAccountId == currentId.Id)
                        .First().Amount = item.Amount;

                }
                db.CollectionLogs.AddRange(newItems);
            }
            await db.SaveChangesAsync();

            return SyncOsrsAccountCollectionResult.Failure;

        }
    }
}
