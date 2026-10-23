namespace NivBot.DataLayer.Enums
{
    // What a role rule measures. Stored as a number, don't renumber.
    // KC-like metrics are added up over all of a member's linked accounts,
    // level-like metrics use the member's best single account.
    public enum RoleMetric
    {
        // KC / score of one activity (DiscordRole.ActivityId), e.g. Vorkath KC or Clue Scrolls (all). Added up.
        ActivityScore = 0,
        // Total level of the best account.
        TotalLevel = 1,
        // Total XP of the best account.
        TotalXp = 2,
        // Kill count over all bosses (see the Bosses table). Added up.
        TotalBossKc = 3,
        // Completions of all raids (Bosses.IsRaid). Added up.
        RaidsCompleted = 4,
        // Number of skills at level 99 on the best account.
        SkillsAt99 = 5,
        // Number of skills at 200m XP on the best account.
        SkillsAt200m = 6,
        // Level in one skill (DiscordRole.Skill) on the best account.
        SkillLevel = 7,
        // Times an item (DiscordRole.ItemId) is in the collection log, from the Temple sync. Added up.
        CollectionLogItem = 8
    }
}
