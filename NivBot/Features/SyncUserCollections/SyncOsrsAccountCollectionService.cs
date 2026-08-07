using Microsoft.EntityFrameworkCore;
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
            return SyncOsrsAccountCollectionResult.FailureNotImplemented;
        }
        public async Task<SyncOsrsAccountCollectionResult> SyncGroupAccountCollog(int groupId)
        {
            var groupCollog = await templeApi.GetGroupCollectionsAsync(groupId);
            var dbCollogs = await db.CollectionLogs.ToListAsync();
            var dbItems = await db.Items.ToListAsync();
            foreach (var osrsCharacter in groupCollog)
            {
                // Continue to next iteration if character doesn't exist
                if (!dbCollogs.Any(y => y.RunescapeAccount.RunescapeName == osrsCharacter.OsrsName))
                { continue; }

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
                        .Where(y => y.RunescapeAccount.RunescapeName == osrsCharacter.OsrsName)
                        .Any(x => x.Item.OsrsId == item.OsrsId)
                        )
                    {
                        newItems.Add(
                            new CollectionLog
                            {
                                Amount = item.Amount,
                                ItemId = dbItems.First(y => y.OsrsId == item.OsrsId).Id,
                                RunescapeAccountId = dbCollogs.First(y => y.RunescapeAccount.RunescapeName == osrsCharacter.OsrsName).RunescapeAccountId
                            }
                        );
                        continue;
                    }

                    // Set the amount of the item
                    dbCollogs
                        .Where(y => y.Item.OsrsId == item.OsrsId)
                        .Where(z => z.RunescapeAccount.RunescapeName == osrsCharacter.OsrsName)
                        .First().Amount = item.Amount;

                }
                dbCollogs.AddRange(newItems);
            }
            await db.SaveChangesAsync();

            return SyncOsrsAccountCollectionResult.Failure;

        }
    }
}
