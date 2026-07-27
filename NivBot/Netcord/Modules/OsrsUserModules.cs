using Microsoft.EntityFrameworkCore;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NivBot.DataLayer;
using NivBot.DataLayer.Models;
using NivBot.ExternalServicesLayer.OsrsAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.Netcord.Modules
{
    public class OsrsUserModules(IOsrsHighscoreService osrsApi, GoodplaceContext db) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SlashCommand("addcharacter", "Attempt to link an Old School Runescape account to your Discord Account.")]
        public async Task AddOsrsAccountAsync([SlashCommandParameter(Name = "username", Description = "Your Old School Runescape character name")] string username)
        {
            bool accountExists = await db.RunescapeAccounts.AnyAsync(x => x.RunescapeName == username);
            bool user = await db.GoodplaceUsers.AnyAsync(u => u.DiscordUserId == (long)Context.User.Id);
            if (accountExists)
            {

            }
            if (!user) { return; }
            
        }

    }
}
