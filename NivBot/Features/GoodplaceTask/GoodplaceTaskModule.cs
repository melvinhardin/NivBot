using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.Features.GoodplaceTask
{
    public class GoodplaceTaskModule(GoodplaceTaskService goodplaceTask) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("skilltask", "Complete, Assign, Block or skip a skill task")]
        public async Task getSkillTaskMenuAsync() {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(NetCord.MessageFlags.Ephemeral));
            var reply = Context.Interaction.SendResponseAsync(
            InteractionCallback.Message(new InteractionMessageProperties
            {
                Content = "...",
                Flags = NetCord.MessageFlags.Ephemeral,
                Components = [
                    new ActionRowProperties{
                        new ButtonProperties("skip", "Skip Task", NetCord.ButtonStyle.Secondary),
                        new ButtonProperties("block", "Block Task", NetCord.ButtonStyle.Danger),
                    }
                    ]
            })


            );
    }
}
