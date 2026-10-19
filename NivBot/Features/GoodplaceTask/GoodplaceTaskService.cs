using Microsoft.EntityFrameworkCore;
using NivBot.DataLayer;
using NivBot.DataLayer.Enums;
using NivBot.DataLayer.Models;
using NivBot.Features.Shared;

namespace NivBot.Features.GoodplaceTask
{
    // Skill and boss tasks. Progress is the summed XP / kill count over all of the user's linked accounts,
    // refreshed from the hiscores every time the user interacts with a task.
    public class GoodplaceTaskService(
        GoodplaceContext db,
        OsrsAccountStatsService statsService,
        TaskGoalCalculator goalCalculator,
        TaskOptions options)
    {
        // Shows the current task. Assigns one when there is none, and when the goal is reached
        // awards points and assigns the next one.
        public async Task<GoodplaceTaskResult> CheckTaskAsync(long discordId, TaskType type)
        {
            GoodplaceUser? user = await LoadUserAsync(discordId);
            if (user == null) { return new(GoodplaceTaskStatus.NotRegistered); }

            var (refresh, accounts) = await statsService.RefreshUserAccountsAsync(user.Id);
            if (accounts.Count == 0) { return new(GoodplaceTaskStatus.NoAccounts); }
            if (refresh == StatsRefreshResult.HiscoresUnavailable) { return new(GoodplaceTaskStatus.HiscoresUnavailable); }

            TaskProgress? current = await GetProgressAsync(user, accounts, type);
            if (current == null)
            {
                TaskProgress? assigned = await AssignTaskAsync(user, accounts, type, exclude: null, allowRepeat: true);
                await db.SaveChangesAsync();
                return assigned == null
                    ? new(GoodplaceTaskStatus.NoTasksAvailable)
                    : new(GoodplaceTaskStatus.Assigned, assigned);
            }

            if (current.IsComplete) { return await CompleteTaskAsync(user, accounts, type, current); }

            // Saves the refreshed stats
            await db.SaveChangesAsync();
            return new(GoodplaceTaskStatus.InProgress, current);
        }

        // Replaces the current task with a different one, once per TaskOptions.SkipCooldown per task type.
        public async Task<GoodplaceTaskResult> SkipTaskAsync(long discordId, TaskType type)
        {
            GoodplaceUser? user = await LoadUserAsync(discordId);
            if (user == null) { return new(GoodplaceTaskStatus.NotRegistered); }

            DateTime? lastSkip = type == TaskType.Skill ? user.GoodplaceSkillTask?.LastSkippedAt : user.GoodplaceActivityTask?.LastSkippedAt;
            int? currentKey = CurrentTaskKey(user, type);
            if (currentKey == null) { return new(GoodplaceTaskStatus.NoActiveTask); }

            if (lastSkip != null)
            {
                TimeSpan remaining = lastSkip.Value + options.SkipCooldown - DateTime.UtcNow;
                if (remaining > TimeSpan.Zero) { return new(GoodplaceTaskStatus.SkipOnCooldown, CooldownRemaining: remaining); }
            }

            var (refresh, accounts) = await statsService.RefreshUserAccountsAsync(user.Id);
            if (accounts.Count == 0) { return new(GoodplaceTaskStatus.NoAccounts); }
            if (refresh == StatsRefreshResult.HiscoresUnavailable) { return new(GoodplaceTaskStatus.HiscoresUnavailable); }

            // Don't let a finished task be skipped away without its reward (and without using the skip)
            TaskProgress? current = await GetProgressAsync(user, accounts, type);
            if (current is { IsComplete: true }) { return await CompleteTaskAsync(user, accounts, type, current); }

            string previousName = await GetTaskNameAsync(type, currentKey.Value);
            TaskProgress? next = await AssignTaskAsync(user, accounts, type, exclude: currentKey, allowRepeat: false);
            if (next == null) { return new(GoodplaceTaskStatus.NoTasksAvailable); }

            if (type == TaskType.Skill) { user.GoodplaceSkillTask!.LastSkippedAt = DateTime.UtcNow; }
            else { user.GoodplaceActivityTask!.LastSkippedAt = DateTime.UtcNow; }

            await db.SaveChangesAsync();
            return new(GoodplaceTaskStatus.Skipped, next, previousName);
        }

        // Adds the current task to the user's blocklist (capped per task type) and assigns a different one.
        public async Task<GoodplaceTaskResult> BlockTaskAsync(long discordId, TaskType type)
        {
            GoodplaceUser? user = await LoadUserAsync(discordId);
            if (user == null) { return new(GoodplaceTaskStatus.NotRegistered); }

            int? currentKey = CurrentTaskKey(user, type);
            if (currentKey == null) { return new(GoodplaceTaskStatus.NoActiveTask); }

            int blockCount = type == TaskType.Skill
                ? await db.SkillTaskBlockLists.CountAsync(x => x.GoodplaceUserId == user.Id)
                : await db.ActivityTaskBlockLists.CountAsync(x => x.GoodplaceUserId == user.Id);
            int maxBlocks = type == TaskType.Skill ? options.MaxSkillBlocks : options.MaxBossBlocks;
            if (blockCount >= maxBlocks) { return new(GoodplaceTaskStatus.BlockLimitReached, BlockLimit: maxBlocks); }

            var (refresh, accounts) = await statsService.RefreshUserAccountsAsync(user.Id);
            if (accounts.Count == 0) { return new(GoodplaceTaskStatus.NoAccounts); }
            if (refresh == StatsRefreshResult.HiscoresUnavailable) { return new(GoodplaceTaskStatus.HiscoresUnavailable); }

            // A finished task is completed instead of blocked
            TaskProgress? current = await GetProgressAsync(user, accounts, type);
            if (current is { IsComplete: true }) { return await CompleteTaskAsync(user, accounts, type, current); }

            string previousName = await GetTaskNameAsync(type, currentKey.Value);
            TaskProgress? next = await AssignTaskAsync(user, accounts, type, exclude: currentKey, allowRepeat: false);
            if (next == null) { return new(GoodplaceTaskStatus.NoTasksAvailable); }

            if (type == TaskType.Skill)
            {
                db.SkillTaskBlockLists.Add(new SkillTaskBlocklist { GoodplaceUserId = user.Id, Skill = (Skills)currentKey.Value });
            }
            else
            {
                db.ActivityTaskBlockLists.Add(new ActivityTaskBlocklist { GoodplaceUserId = user.Id, ActivityId = currentKey.Value });
            }

            await db.SaveChangesAsync();
            return new(GoodplaceTaskStatus.Blocked, next, previousName);
        }

        public async Task<List<BlockedTask>> GetBlocksAsync(long discordId)
        {
            int? userId = await GetUserIdAsync(discordId);
            if (userId == null) { return []; }

            List<Skills> skills = await db.SkillTaskBlockLists
                .Where(x => x.GoodplaceUserId == userId)
                .Select(x => x.Skill)
                .ToListAsync();
            var bosses = await db.ActivityTaskBlockLists
                .Where(x => x.GoodplaceUserId == userId)
                .Select(x => new { x.ActivityId, x.Activity.OsrsName })
                .ToListAsync();

            return skills
                .OrderBy(x => x.ToString())
                .Select(x => new BlockedTask(TaskType.Skill, (int)x, x.ToString()))
                .Concat(bosses
                    .OrderBy(x => x.OsrsName)
                    .Select(x => new BlockedTask(TaskType.Boss, x.ActivityId, x.OsrsName)))
                .ToList();
        }

        public async Task<int> UnblockAsync(long discordId, IEnumerable<(TaskType Type, int Id)> tasks)
        {
            int? userId = await GetUserIdAsync(discordId);
            if (userId == null) { return 0; }

            int removed = 0;
            foreach (var (type, id) in tasks)
            {
                if (type == TaskType.Skill)
                {
                    Skills skill = (Skills)id;
                    removed += await db.SkillTaskBlockLists
                        .Where(x => x.GoodplaceUserId == userId && x.Skill == skill)
                        .ExecuteDeleteAsync();
                }
                else
                {
                    removed += await db.ActivityTaskBlockLists
                        .Where(x => x.GoodplaceUserId == userId && x.ActivityId == id)
                        .ExecuteDeleteAsync();
                }
            }
            return removed;
        }

        // When a new account is linked, its current XP / kill count must not count towards a running task.
        // Moves the baseline and the goal of the user's tasks up by what the new account already has.
        // Does not save: called by LinkOsrsAccountService before its own save.
        public async Task ShiftTasksForNewAccountAsync(int goodplaceUserId, RunescapeAccount newAccount)
        {
            GoodplaceSkillTask? skillTask = await db.GoodplaceSkillTasks.FirstOrDefaultAsync(x => x.GoodplaceUserId == goodplaceUserId);
            if (skillTask != null)
            {
                long xp = newAccount.RunescapeStats
                    .Where(x => x.Skill == skillTask.Skill)
                    .Sum(x => (long)x.Xp);
                skillTask.SummedCurrentXp += xp;
                skillTask.GoalXp += xp;
            }

            GoodplaceActivityTask? bossTask = await db.GoodplaceActivityTasks.FirstOrDefaultAsync(x => x.GoodplaceUserId == goodplaceUserId);
            if (bossTask != null)
            {
                int kc = newAccount.ActivityLogs
                    .Where(x => x.ActivityId == bossTask.ActivityId)
                    .Sum(x => x.Amount);
                bossTask.CurrentCumulativeAmount += kc;
                bossTask.GoalAmount += kc;
            }
        }

        // Awards points for a finished task and assigns the next one.
        private async Task<GoodplaceTaskResult> CompleteTaskAsync(GoodplaceUser user, List<RunescapeAccount> accounts, TaskType type, TaskProgress current)
        {
            // A goal at or below the baseline can only come from old test data (goal 1), replace it without a reward.
            bool validTask = current.Required > 0;
            if (validTask)
            {
                Wallet wallet = user.Wallet ??= new Wallet { GoodplaceUserId = user.Id };
                wallet.GoodplacePoints += options.PointsPerTask;
                wallet.GoodplaceCurrency += options.CurrencyPerTask;
            }

            TaskProgress? next = await AssignTaskAsync(user, accounts, type, exclude: CurrentTaskKey(user, type), allowRepeat: true);
            if (next == null) { RemoveTask(user, type); }
            await db.SaveChangesAsync();

            if (!validTask)
            {
                return next == null ? new(GoodplaceTaskStatus.NoTasksAvailable) : new(GoodplaceTaskStatus.Assigned, next);
            }
            return new(GoodplaceTaskStatus.Completed, next, current.Name, options.PointsPerTask, options.CurrencyPerTask);
        }

        private Task<GoodplaceUser?> LoadUserAsync(long discordId) =>
            db.GoodplaceUsers
                .Include(x => x.Wallet)
                .Include(x => x.GoodplaceSkillTask)
                .Include(x => x.GoodplaceActivityTask)
                .FirstOrDefaultAsync(x => x.DiscordUserId == discordId);

        private Task<int?> GetUserIdAsync(long discordId) =>
            db.GoodplaceUsers
                .Where(x => x.DiscordUserId == discordId)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();

        // Skill tasks are keyed by the Skills enum value, boss tasks by Activity.Id.
        private static int? CurrentTaskKey(GoodplaceUser user, TaskType type) => type == TaskType.Skill
            ? (int?)user.GoodplaceSkillTask?.Skill
            : user.GoodplaceActivityTask?.ActivityId;

        private void RemoveTask(GoodplaceUser user, TaskType type)
        {
            if (type == TaskType.Skill && user.GoodplaceSkillTask != null) { db.GoodplaceSkillTasks.Remove(user.GoodplaceSkillTask); }
            if (type == TaskType.Boss && user.GoodplaceActivityTask != null) { db.GoodplaceActivityTasks.Remove(user.GoodplaceActivityTask); }
        }

        private async Task<string> GetTaskNameAsync(TaskType type, int key)
        {
            if (type == TaskType.Skill) { return ((Skills)key).ToString(); }

            string? name = await db.Activities
                .Where(x => x.Id == key)
                .Select(x => x.OsrsName)
                .FirstOrDefaultAsync();
            return name ?? "Unknown boss";
        }

        private async Task<TaskProgress?> GetProgressAsync(GoodplaceUser user, List<RunescapeAccount> accounts, TaskType type)
        {
            if (type == TaskType.Skill)
            {
                GoodplaceSkillTask? task = user.GoodplaceSkillTask;
                if (task == null) { return null; }

                long gained = SummedXp(accounts, task.Skill) - task.SummedCurrentXp;
                return new TaskProgress(TaskType.Skill, task.Skill.ToString(), Math.Max(gained, 0), task.GoalXp - task.SummedCurrentXp);
            }

            GoodplaceActivityTask? bossTask = user.GoodplaceActivityTask;
            if (bossTask == null) { return null; }

            long kills = SummedKc(accounts, bossTask.ActivityId) - bossTask.CurrentCumulativeAmount;
            string name = await GetTaskNameAsync(TaskType.Boss, bossTask.ActivityId);
            return new TaskProgress(TaskType.Boss, name, Math.Max(kills, 0), bossTask.GoalAmount - bossTask.CurrentCumulativeAmount);
        }

        // Picks a random task and stores it on the user (new row or replacing the current one).
        // exclude: the current task key, so the new task is a different one.
        // allowRepeat: when nothing else is available, the excluded task may be assigned again.
        // Returns null when there is nothing to assign; nothing is changed in that case.
        private Task<TaskProgress?> AssignTaskAsync(GoodplaceUser user, List<RunescapeAccount> accounts, TaskType type, int? exclude, bool allowRepeat) =>
            type == TaskType.Skill
                ? AssignSkillTaskAsync(user, accounts, exclude, allowRepeat)
                : AssignBossTaskAsync(user, accounts, exclude, allowRepeat);

        private async Task<TaskProgress?> AssignSkillTaskAsync(GoodplaceUser user, List<RunescapeAccount> accounts, int? exclude, bool allowRepeat)
        {
            Dictionary<Skills, List<SkillRate>> ratesBySkill = (await db.SkillRates.AsNoTracking().ToListAsync())
                .Where(x => x.XpPerHour > 0)
                .GroupBy(x => x.Skill)
                .ToDictionary(x => x.Key, x => x.ToList());

            HashSet<Skills> blocked = (await db.SkillTaskBlockLists
                .Where(x => x.GoodplaceUserId == user.Id)
                .Select(x => x.Skill)
                .ToListAsync())
                .ToHashSet();
            blocked.UnionWith(options.GetExcludedSkills());

            // Only skills with rates and with at least one account that can still gain XP in them
            List<Skills> candidates = Enum.GetValues<Skills>()
                .Where(x => !blocked.Contains(x) && ratesBySkill.ContainsKey(x) && ReferenceXp(accounts, x) != null)
                .ToList();

            List<Skills> pool = candidates.Where(x => exclude == null || (int)x != exclude.Value).ToList();
            if (pool.Count == 0 && allowRepeat) { pool = candidates; }
            if (pool.Count == 0) { return null; }

            Skills skill = pool[Random.Shared.Next(pool.Count)];
            long baseline = SummedXp(accounts, skill);
            // Rates depend on level, use the highest account that can still gain XP (most likely the one being trained)
            long xpToGain = goalCalculator.XpToGain(ratesBySkill[skill], ReferenceXp(accounts, skill)!.Value, goalCalculator.RollHours());
            xpToGain = Math.Max(xpToGain, 1);

            GoodplaceSkillTask? task = user.GoodplaceSkillTask;
            if (task == null)
            {
                task = new GoodplaceSkillTask { GoodplaceUserId = user.Id };
                db.GoodplaceSkillTasks.Add(task);
                user.GoodplaceSkillTask = task;
            }
            task.Skill = skill;
            task.SummedCurrentXp = baseline;
            task.GoalXp = baseline + xpToGain;

            return new TaskProgress(TaskType.Skill, skill.ToString(), 0, xpToGain);
        }

        private async Task<TaskProgress?> AssignBossTaskAsync(GoodplaceUser user, List<RunescapeAccount> accounts, int? exclude, bool allowRepeat)
        {
            // Bosses are matched to their rate by name; activities without a rate (clues, minigames...) are never assigned.
            Dictionary<string, BossRate> rates = (await db.BossRates.AsNoTracking().ToListAsync())
                .Where(x => x.KillsPerHour > 0)
                .GroupBy(x => x.OsrsName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

            HashSet<int> blocked = (await db.ActivityTaskBlockLists
                .Where(x => x.GoodplaceUserId == user.Id)
                .Select(x => x.ActivityId)
                .ToListAsync())
                .ToHashSet();
            blocked.UnionWith(await db.GlobalActivityBlockLists.Select(x => x.ActivityId).ToListAsync());

            List<Activity> candidates = (await db.Activities.AsNoTracking().ToListAsync())
                .Where(x => !blocked.Contains(x.Id) && rates.ContainsKey(x.OsrsName))
                .ToList();

            List<Activity> pool = candidates.Where(x => exclude == null || x.Id != exclude.Value).ToList();
            if (pool.Count == 0 && allowRepeat) { pool = candidates; }
            if (pool.Count == 0) { return null; }

            Activity boss = pool[Random.Shared.Next(pool.Count)];
            int baseline = SummedKc(accounts, boss.Id);
            int killsToGet = goalCalculator.KillsToGet(rates[boss.OsrsName], baseline, goalCalculator.RollHours());

            GoodplaceActivityTask? task = user.GoodplaceActivityTask;
            if (task == null)
            {
                task = new GoodplaceActivityTask { GoodplaceUserId = user.Id };
                db.GoodplaceActivityTasks.Add(task);
                user.GoodplaceActivityTask = task;
            }
            task.ActivityId = boss.Id;
            task.CurrentCumulativeAmount = baseline;
            task.GoalAmount = baseline + killsToGet;

            return new TaskProgress(TaskType.Boss, boss.OsrsName, 0, killsToGet);
        }

        private static long SummedXp(List<RunescapeAccount> accounts, Skills skill) =>
            accounts.Sum(a => (long)(a.RunescapeStats.FirstOrDefault(s => s.Skill == skill)?.Xp ?? 0));

        private static int SummedKc(List<RunescapeAccount> accounts, int activityId) =>
            accounts.Sum(a => a.ActivityLogs.FirstOrDefault(l => l.ActivityId == activityId)?.Amount ?? 0);

        // Highest XP in the skill among accounts that are still below 200m, null when every account is maxed.
        private static long? ReferenceXp(List<RunescapeAccount> accounts, Skills skill)
        {
            List<long> xps = accounts
                .Select(a => (long)(a.RunescapeStats.FirstOrDefault(s => s.Skill == skill)?.Xp ?? 0))
                .Where(xp => xp < OsrsAccountStatsService.MaxXp)
                .ToList();
            return xps.Count == 0 ? null : xps.Max();
        }
    }
}
