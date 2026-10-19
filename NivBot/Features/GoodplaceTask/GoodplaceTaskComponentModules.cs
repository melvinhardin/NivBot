using Microsoft.Extensions.Logging;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace NivBot.Features.GoodplaceTask
{
    // Skip and block buttons under /skilltask and /bosstask. The messages are ephemeral,
    // so only the user who ran the command can press them.
    public class GoodplaceTaskButtonModule(GoodplaceTaskService taskService, ILogger<GoodplaceTaskButtonModule> logger) : ComponentInteractionModule<ButtonInteractionContext>
    {
        [ComponentInteraction(TaskMessages.SkipId)]
        public Task SkipAsync(TaskType type) =>
            RunAsync(type, "skip", () => taskService.SkipTaskAsync((long)Context.User.Id, type));

        [ComponentInteraction(TaskMessages.BlockId)]
        public Task BlockAsync(TaskType type) =>
            RunAsync(type, "block", () => taskService.BlockTaskAsync((long)Context.User.Id, type));

        private async Task RunAsync(TaskType type, string action, Func<Task<GoodplaceTaskResult>> run)
        {
            // Acknowledge first: the hiscores lookup can take longer than Discord's 3 second limit.
            await RespondAsync(InteractionCallback.DeferredModifyMessage);

            GoodplaceTaskResult result;
            try
            {
                result = await run();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{Type} task {Action} failed for {User}", type, action, Context.User.Id);
                result = new GoodplaceTaskResult(GoodplaceTaskStatus.Error);
            }

            await ModifyResponseAsync(message =>
            {
                message.Content = TaskMessages.Render(result, type);
                message.Components = TaskMessages.Components(result, type);
            });
        }
    }

    // The select menu under /taskblocks.
    public class GoodplaceTaskMenuModule(GoodplaceTaskService taskService, ILogger<GoodplaceTaskMenuModule> logger) : ComponentInteractionModule<StringMenuInteractionContext>
    {
        [ComponentInteraction(TaskMessages.UnblockId)]
        public async Task UnblockAsync()
        {
            await RespondAsync(InteractionCallback.DeferredModifyMessage);

            string content;
            IEnumerable<IMessageComponentProperties> components = [];
            try
            {
                var selected = new List<(TaskType Type, int Id)>();
                foreach (string value in Context.SelectedValues)
                {
                    if (TaskMessages.TryParseBlockValue(value, out var task)) { selected.Add(task); }
                }

                int removed = await taskService.UnblockAsync((long)Context.User.Id, selected);
                List<BlockedTask> remaining = await taskService.GetBlocksAsync((long)Context.User.Id);

                content = $"Unblocked {removed} task{(removed == 1 ? "" : "s")}. {TaskMessages.BlocksText(remaining)}";
                components = TaskMessages.BlocksMenu(remaining);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unblocking tasks failed for {User}", Context.User.Id);
                content = "Something went wrong, try again later.";
            }

            await ModifyResponseAsync(message =>
            {
                message.Content = content;
                message.Components = components;
            });
        }
    }
}
