using Microsoft.EntityFrameworkCore;
using NivBot.DataLayer;
using NivBot.ExternalServicesLayer.TempleAPI;

namespace NivBot.Features.SyncCollectionList
{
    public class SyncCollectionListService(GoodplaceContext db, ITempleService templeApi)
    {
        public async Task<SyncCollectionListResult> SyncItemList()
        {
            // Get both item data lists. Return relevant failures if any connection fails.
            List<DataLayer.Models.Item> currentDbItems;
            try
            {
                currentDbItems = await db.Items.ToListAsync();
            }
            catch(Exception ex)
            {
                return SyncCollectionListResult.FailureDbConnection;
            }

            Dictionary<int, string> currentTempleItems;
            try 
            {
                currentTempleItems = await templeApi.GetItemListAsync();
            }
            catch(Exception ex) 
            {
                return SyncCollectionListResult.FailureTempleApiConnection;
            }
            
            // Make a dictionary from the db list on the osrs item id.
            var dbDict = currentDbItems.ToDictionary(x => x.OsrsId);

            // Check if item in Db
            var addedItems = currentTempleItems.Keys.Except(dbDict.Keys).ToList();

            // TODO add functionality to update all the itemnames in the db.

            // Add new Items 
            List<NivBot.DataLayer.Models.Item> newItems = addedItems.Select(x => new NivBot.DataLayer.Models.Item
            {
                OsrsId = x,
                OsrsName = currentTempleItems[x],
                Points = 0
            }).ToList();
            db.Items.AddRange(newItems);
            try { await db.SaveChangesAsync(); }
            catch (Exception ex) { return SyncCollectionListResult.FailureDbSave; }

            // Return Success state
            return SyncCollectionListResult.SuccessSync;
        }
    }
}
