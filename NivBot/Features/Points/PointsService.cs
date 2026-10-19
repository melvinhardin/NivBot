using Microsoft.EntityFrameworkCore;
using NivBot.DataLayer;
using System.Globalization;

namespace NivBot.Features.Points
{
    public sealed record WalletView(int Points, int Currency, int Rank);
    public sealed record LeaderboardEntry(long DiscordUserId, int Points);

    public class PointsService(GoodplaceContext db)
    {
        // Item.Points and Activity.Points are limited to 0-1000 by check constraints.
        public const int MaxPoints = 1000;

        // Null when the user isn't registered. Rank is 1-based by points.
        public async Task<WalletView?> GetWalletAsync(long discordId)
        {
            var wallet = await db.Wallets
                .Where(x => x.GoodplaceUser.DiscordUserId == discordId)
                .Select(x => new { x.GoodplacePoints, x.GoodplaceCurrency })
                .FirstOrDefaultAsync();
            if (wallet == null)
            {
                bool registered = await db.GoodplaceUsers.AnyAsync(x => x.DiscordUserId == discordId);
                return registered ? new WalletView(0, 0, 0) : null;
            }

            int better = await db.Wallets.CountAsync(x => x.GoodplacePoints > wallet.GoodplacePoints);
            return new WalletView(wallet.GoodplacePoints, wallet.GoodplaceCurrency, better + 1);
        }

        public Task<List<LeaderboardEntry>> GetLeaderboardAsync(int count) =>
            db.Wallets
                .OrderByDescending(x => x.GoodplacePoints)
                .Take(count)
                .Select(x => new LeaderboardEntry(x.GoodplaceUser.DiscordUserId, x.GoodplacePoints))
                .ToListAsync();

        // item: the OSRS id from the autocomplete, or a name. Returns the item's name, null when not found.
        public async Task<string?> SetItemPointsAsync(string item, int points)
        {
            DataLayer.Models.Item? found = int.TryParse(item, NumberStyles.Integer, CultureInfo.InvariantCulture, out int osrsId)
                ? await db.Items.FirstOrDefaultAsync(x => x.OsrsId == osrsId)
                : await db.Items.FirstOrDefaultAsync(x => x.OsrsName.ToLower() == item.Trim().ToLower());
            if (found == null) { return null; }

            found.Points = points;
            await db.SaveChangesAsync();
            return found.OsrsName;
        }

        // Activity.Points: bonus points and currency for completing a boss task of this boss.
        // Returns the activity's name, null when not found.
        public async Task<string?> SetBossPointsAsync(string boss, int points)
        {
            string name = boss.Trim().ToLower();
            DataLayer.Models.Activity? found = await db.Activities.FirstOrDefaultAsync(x => x.OsrsName.ToLower() == name);
            if (found == null) { return null; }

            found.Points = points;
            await db.SaveChangesAsync();
            return found.OsrsName;
        }
    }
}
