using Microsoft.EntityFrameworkCore;
using NivBot.DataLayer;
using NivBot.ExternalServicesLayer.TempleAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.Features.SyncCollectionList
{
    public class SyncCollectionListService(GoodplaceContext db, ITempleService templeApi)
    {
        public async Task SyncItemList()
        {
            // Get both lists
            var currentDbItems = await db.Items.ToListAsync();
            var currentTempleItems = await templeApi.GetItemListAsync();

            // Make a dictionary from the db list.
            var dbDict = currentDbItems.ToDictionary(x => x.OsrsId);

            // Check if item in Db
            var newItems = new Dictionary<string, DataLayer.Models.Item>();


            foreach(var x in currentTempleItems)
            {
                Int32.TryParse(x.Key, out int z);
                currentDbItems
            }

        }
    }
}
