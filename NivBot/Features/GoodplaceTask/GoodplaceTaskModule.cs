using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace NivBot.Features.GoodplaceTask
{
    public class GoodplaceTaskModule(GoodplaceTaskService taskService, ILogger<GoodplaceTaskModule> logger) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("skilltask", "Show your skill task, or get a new one when it's done")]
        public Task SkillTaskAsync() => ShowTaskAsync(TaskType.Skill);

        [SlashCommand("bosstask", "Show your boss task, or get a new one when it's done")]
        public Task BossTaskAsync() => ShowTaskAsync(TaskType.Boss);

        [SlashCommand("taskblocks", "Show and remove your blocked tasks")]
        public async Task TaskBlocksAsync()
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            string content;
            IEnumerable<IMessageComponentProperties> components = [];
            try
            {
                List<BlockedTask> blocks = await taskService.GetBlocksAsync((long)Context.User.Id);
                content = TaskMessages.BlocksText(blocks);
                components = TaskMessages.BlocksMenu(blocks);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "/taskblocks failed for {User}", Context.User.Id);
                content = "Something went wrong, try again later.";
            }

            await ModifyResponseAsync(message =>
            {
                message.Content = content;
                message.Components = components;
            });
        }

        private async Task ShowTaskAsync(TaskType type)
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

            GoodplaceTaskResult result;
            try
            {
                result = await taskService.CheckTaskAsync((long)Context.User.Id, type);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{Type} task check failed for {User}", type, Context.User.Id);
                result = new GoodplaceTaskResult(GoodplaceTaskStatus.Error);
            }

            await ModifyResponseAsync(message =>
            {
                message.Content = TaskMessages.Render(result, type);
                message.Components = TaskMessages.Components(result, type);
            });
        }
    }
}
