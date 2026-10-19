using NetCord;
using NetCord.Rest;
using System.Globalization;

namespace NivBot.Features.GoodplaceTask
{
    // Discord texts and components for the task commands, shared by the slash command and component modules.
    internal static class TaskMessages
    {
        // Custom ids: "<id>:<(int)TaskType>". NetCord reads enum parameters by their numeric value.
        public const string SkipId = "task-skip";
        public const string BlockId = "task-block";
        public const string UnblockId = "task-unblock";

        // Discord allows 25 options in a select menu.
        private const int MaxMenuOptions = 25;

        public static string Render(GoodplaceTaskResult result, TaskType type)
        {
            string kind = type == TaskType.Skill ? "skill" : "boss";
            string command = type == TaskType.Skill ? "/skilltask" : "/bosstask";

            return result.Status switch
            {
                GoodplaceTaskStatus.NotRegistered => "You are not registered yet, use /register first.",
                GoodplaceTaskStatus.NoAccounts => "Link an Old School RuneScape account first with /addcharacter.",
                GoodplaceTaskStatus.HiscoresUnavailable => "The OSRS hiscores can't be reached right now, try again in a few minutes.",
                GoodplaceTaskStatus.NoTasksAvailable => $"There are no other {kind} tasks to assign. Remove blocks with /taskblocks, or ask an admin to check the task settings.",
                GoodplaceTaskStatus.NoActiveTask => $"You don't have a {kind} task, use {command} to get one.",
                GoodplaceTaskStatus.Assigned => NewTask(result.Task),
                GoodplaceTaskStatus.InProgress => Progress(result.Task!),
                GoodplaceTaskStatus.Completed =>
                    $"Task complete: **{result.PreviousTaskName}**! +{result.PointsAwarded} points, +{result.CurrencyAwarded} currency.\n" +
                    (result.Task == null ? $"There are no other {kind} tasks to assign." : NewTask(result.Task)),
                GoodplaceTaskStatus.Skipped => $"Skipped **{result.PreviousTaskName}**.\n{NewTask(result.Task)}",
                GoodplaceTaskStatus.SkipOnCooldown => $"You can skip a {kind} task again in {Duration(result.CooldownRemaining ?? TimeSpan.Zero)}.",
                GoodplaceTaskStatus.Blocked => $"Blocked **{result.PreviousTaskName}**, undo with /taskblocks.\n{NewTask(result.Task)}",
                GoodplaceTaskStatus.BlockLimitReached => $"You already have the maximum of {result.BlockLimit} blocked {kind} tasks. Remove one with /taskblocks first.",
                _ => "Something went wrong, try again later."
            };
        }

        // Skip and block buttons, shown whenever the user has a task to act on.
        public static IEnumerable<IMessageComponentProperties> Components(GoodplaceTaskResult result, TaskType type)
        {
            bool hasTask = result.Status switch
            {
                GoodplaceTaskStatus.Assigned or
                GoodplaceTaskStatus.InProgress or
                GoodplaceTaskStatus.Skipped or
                GoodplaceTaskStatus.Blocked or
                GoodplaceTaskStatus.SkipOnCooldown or
                GoodplaceTaskStatus.BlockLimitReached => true,
                GoodplaceTaskStatus.Completed => result.Task != null,
                _ => false
            };
            if (!hasTask) { return []; }

            return
            [
                new ActionRowProperties
                {
                    new ButtonProperties($"{SkipId}:{(int)type}", "Skip", ButtonStyle.Secondary),
                    new ButtonProperties($"{BlockId}:{(int)type}", "Block", ButtonStyle.Danger),
                }
            ];
        }

        public static string BlocksText(IReadOnlyList<BlockedTask> blocks) => blocks.Count == 0
            ? "You have no blocked tasks."
            : "Your blocked tasks are below, select the ones to unblock.";

        public static IEnumerable<IMessageComponentProperties> BlocksMenu(IReadOnlyList<BlockedTask> blocks)
        {
            if (blocks.Count == 0) { return []; }

            List<BlockedTask> shown = blocks.Take(MaxMenuOptions).ToList();
            return
            [
                new StringMenuProperties(UnblockId, shown.Select(x => new StringMenuSelectOptionProperties(
                    $"{(x.Type == TaskType.Skill ? "Skill" : "Boss")}: {x.Name}",
                    $"{(int)x.Type}:{x.Id}")))
                {
                    Placeholder = "Select tasks to unblock",
                    MinValues = 1,
                    MaxValues = shown.Count
                }
            ];
        }

        // Reads a menu value written by BlocksMenu ("<(int)TaskType>:<id>").
        public static bool TryParseBlockValue(string value, out (TaskType Type, int Id) task)
        {
            task = default;
            string[] parts = value.Split(':');
            if (parts.Length != 2
                || !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int type)
                || !Enum.IsDefined(typeof(TaskType), type)
                || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
            {
                return false;
            }
            task = ((TaskType)type, id);
            return true;
        }

        private static string NewTask(TaskProgress? task)
        {
            if (task == null) { return "There are no other tasks to assign."; }
            return task.Type == TaskType.Skill
                ? $"Your new skill task: gain **{Number(task.Required)} XP** in **{task.Name}**."
                : $"Your new boss task: get **{Number(task.Required)}** kills of **{task.Name}**.";
        }

        private static string Progress(TaskProgress task)
        {
            long percent = task.Required <= 0 ? 100 : Math.Min(100, task.Gained * 100 / task.Required);
            string unit = task.Type == TaskType.Skill ? "XP" : "kills";
            string kind = task.Type == TaskType.Skill ? "Skill" : "Boss";
            return $"{kind} task: **{task.Name}**, {Number(task.Gained)} / {Number(task.Required)} {unit} ({percent}%).";
        }

        private static string Number(long value) => value.ToString("N0", CultureInfo.InvariantCulture);

        private static string Duration(TimeSpan time)
        {
            if (time.TotalHours >= 1) { return $"{(int)time.TotalHours}h {time.Minutes}m"; }
            return $"{Math.Max(1, (int)Math.Ceiling(time.TotalMinutes))}m";
        }
    }
}
