using Microsoft.EntityFrameworkCore;
using NivBot.DataLayer;
using NivBot.DataLayer.Models;
using NivBot.ExternalServicesLayer.TempleAPI;
using NivBot.ExternalServicesLayer.TempleAPI.Models;
using NivBot.Features.Shared;
using System.Text.Json;

namespace NivBot.Features.SyncUserCollections
{
    // Copies collection logs from TempleOSRS into CollectionLogs and awards Item.Points for new items.
    // The first sync of an account only records what it already has, points are awarded from the second sync on.
    public class SyncOsrsAccountCollectionService(GoodplaceContext db, ITempleService templeApi)
    {
        // /synccollection: syncs every account the user linked, one Temple lookup per account.
        public async Task<UserCollectionSyncResult> SyncUserCollectionsAsync(long discordId)
        {
            GoodplaceUser? user = await db.GoodplaceUsers
                .Include(x => x.Wallet)
                .FirstOrDefaultAsync(x => x.DiscordUserId == discordId);
            if (user == null) { return new(SyncOsrsAccountCollectionResult.NotRegistered); }

            List<RunescapeAccount> accounts = await db.RunescapeAccounts
                .Where(x => x.GoodplaceUserId == user.Id)
                .ToListAsync();
            if (accounts.Count == 0) { return new(SyncOsrsAccountCollectionResult.NoAccounts); }

            Wallet wallet = user.Wallet ??= new Wallet { GoodplaceUserId = user.Id };
            Dictionary<int, (int Id, int Points)> itemIds = await GetItemIdsAsync();
            Dictionary<(int, int), CollectionLog> existingLogs = await GetExistingLogsAsync(accounts.Select(x => x.Id).ToArray());

            int synced = 0, notOnTemple = 0, points = 0, firstSyncs = 0;
            foreach (RunescapeAccount account in accounts)
            {
                List<ParsedItem>? items;
                try
                {
                    items = await templeApi.GetPlayerCollectionAsync(account.RunescapeName);
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
                {
                    return new(SyncOsrsAccountCollectionResult.TempleUnavailable);
                }

                if (items == null) { notOnTemple++; continue; }

                if (!account.syncedColLog) { firstSyncs++; }
                points += ApplyItems(account, items, itemIds, existingLogs, wallet);
                synced++;
            }

            await db.SaveChangesAsync();
            return new(SyncOsrsAccountCollectionResult.Success, synced, notOnTemple, points, firstSyncs);
        }

        public async Task<SyncOsrsAccountCollectionResult> SyncGroupAccountCollog(int groupId)
        {
            // Get the list from the API
            List<ParsedMember> groupCollog = await templeApi.GetGroupCollectionsAsync(groupId);

            // Make a hashset from the names to use in the db query (normalized the same way as when linking)
            HashSet<string> names = groupCollog
                .Select(x => OsrsNames.Normalize(x.OsrsName))
                .ToHashSet();

            // Get the relevant names from the db
            Dictionary<string, RunescapeAccount> accounts = await db.RunescapeAccounts
                .Where(x => names.Contains(x.RunescapeName))
                .ToDictionaryAsync(x => x.RunescapeName);

            // Get all userId's of accounts
            int[] userIds = accounts.Values.Select(x => x.GoodplaceUserId).Distinct().ToArray();

            // Get the wallets linked on accountId's
            Dictionary<int, Wallet> wallets = await db.Wallets
                .Where(x => userIds.Contains(x.GoodplaceUserId))
                .ToDictionaryAsync(y => y.GoodplaceUserId);

            Dictionary<int, (int Id, int Points)> itemIds = await GetItemIdsAsync();
            Dictionary<(int, int), CollectionLog> existingLogs = await GetExistingLogsAsync(accounts.Values.Select(x => x.Id).ToArray());

            foreach (var osrsCharacter in groupCollog)
            {
                // Skip to the next character if not in the db
                if (!accounts.TryGetValue(OsrsNames.Normalize(osrsCharacter.OsrsName), out var account)) { continue; }

                if (!wallets.TryGetValue(account.GoodplaceUserId, out Wallet? wallet))
                {
                    wallet = new Wallet { GoodplaceUserId = account.GoodplaceUserId };
                    db.Wallets.Add(wallet);
                    wallets[account.GoodplaceUserId] = wallet;
                }

                ApplyItems(account, osrsCharacter.Items, itemIds, existingLogs, wallet);
            }
            await db.SaveChangesAsync();
            return SyncOsrsAccountCollectionResult.Success;
        }

        // Item OSRS id -> (internal id, points)
        private Task<Dictionary<int, (int Id, int Points)>> GetItemIdsAsync() =>
            db.Items.AsNoTracking().ToDictionaryAsync(x => x.OsrsId, x => (x.Id, x.Points));

        // (account id, item id) -> log
        private Task<Dictionary<(int, int), CollectionLog>> GetExistingLogsAsync(int[] accountIds) =>
            db.CollectionLogs
                .Where(x => accountIds.Contains(x.RunescapeAccountId))
                .ToDictionaryAsync(y => (y.RunescapeAccountId, y.ItemId));

        // Updates the account's collection log with the items from Temple and returns the points awarded.
        private int ApplyItems(
            RunescapeAccount account,
            IEnumerable<ParsedItem> items,
            Dictionary<int, (int Id, int Points)> itemIds,
            Dictionary<(int, int), CollectionLog> existingLogs,
            Wallet wallet)
        {
            int awarded = 0;
            foreach (var item in items)
            {
                // Skip to the next item if not in the db (an admin adds new items with /syncitems)
                if (!itemIds.TryGetValue(item.OsrsId, out var itemId)) { continue; }

                int newAmount = 0;
                if (existingLogs.TryGetValue((account.Id, itemId.Id), out var log))
                {
                    if (log.Amount < item.Amount)
                    {
                        newAmount = item.Amount - log.Amount;
                        log.Amount = item.Amount;
                    }
                }
                else
                {
                    var newCollectionLog = new CollectionLog
                    {
                        Amount = item.Amount,
                        ItemId = itemId.Id,
                        RunescapeAccountId = account.Id
                    };
                    db.CollectionLogs.Add(newCollectionLog);
                    existingLogs[(account.Id, itemId.Id)] = newCollectionLog;
                    newAmount = item.Amount;
                }

                // No points on the first sync, that only records what the account already had
                if (account.syncedColLog && newAmount > 0)
                {
                    awarded += itemId.Points * newAmount;
                }
            }

            wallet.GoodplacePoints += awarded;
            wallet.GoodplaceCurrency += awarded;

            // When we're done adding collectionlogs set the collog flag to synced if it wasn't
            account.syncedColLog = true;
            return awarded;
        }
    }
}
