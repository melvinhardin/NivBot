namespace NivBot.Features.SyncUserCollections
{
    public enum SyncOsrsAccountCollectionResult
    {
        Success,
        Failure,
        NotRegistered,
        NoAccounts,
        TempleUnavailable
    }

    // Result of /synccollection for the user's own accounts.
    public sealed record UserCollectionSyncResult(
        SyncOsrsAccountCollectionResult Status,
        int AccountsSynced = 0,
        int AccountsNotOnTemple = 0,
        int PointsAwarded = 0,
        int FirstSyncs = 0);
}
