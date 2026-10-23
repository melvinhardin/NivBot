namespace NivBot.Features.GoodplaceTask
{
    public enum GoodplaceTaskStatus
    {
        Error,
        NotRegistered,
        NoAccounts,
        HiscoresUnavailable,
        NoTasksAvailable,
        NoActiveTask,
        Assigned,
        InProgress,
        Completed,
        Skipped,
        SkipOnCooldown,
        Blocked,
        BlockLimitReached
    }

    // Progress on a task. Gained and Required are relative to the moment the task was assigned.
    public sealed record TaskProgress(TaskType Type, string Name, long Gained, long Required)
    {
        public bool IsComplete => Gained >= Required;
    }

    // Task: the task the user has after the action (the new one after a completion, skip or block).
    // PreviousTaskName: the completed, skipped or blocked task.
    public sealed record GoodplaceTaskResult(
        GoodplaceTaskStatus Status,
        TaskProgress? Task = null,
        string? PreviousTaskName = null,
        int PointsAwarded = 0,
        int CurrencyAwarded = 0,
        TimeSpan? CooldownRemaining = null,
        int BlockLimit = 0);

    public sealed record BlockedTask(TaskType Type, int Id, string Name);
}
