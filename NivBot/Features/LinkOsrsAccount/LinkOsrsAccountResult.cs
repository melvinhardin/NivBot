namespace NivBot.Features.LinkOsrsAccount
{
    public enum LinkOsrsAccountResult
    {
        FailureUserNotRegistered,
        FailureNotOnHighscores,
        FailureHiscoresUnavailable,
        FailureOsrsNameAlreadyTaken,
        FailureDatabaseSaveFailed,
        SuccessAccountAdded
    }
}
