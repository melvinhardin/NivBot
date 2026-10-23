namespace NivBot.Features.SyncActivities
{
    public enum SyncActivitiesResult
    {
        Success,
        FailureOsrsApiConnection,
        FailureReferencePlayerNotFound,
        FailureDbConnection,
        FailureNameChange,
        FailureDbSave,
        SuccessNoChanges
    }
}
