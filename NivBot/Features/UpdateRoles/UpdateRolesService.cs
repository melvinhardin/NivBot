using Microsoft.EntityFrameworkCore;
using NivBot.DataLayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.Features.UpdateRoles
{
    public class UpdateRolesService (GoodplaceContext db)
    {
        public async Task<UpdateRolesResult> AddRole(int bossId, int threshold, string discordRole)
        {
            DataLayer.Models.Activity activity = await db.Activities.Where(x => x.OsrsId == bossId).FirstOrDefaultAsync();
            if (activity == null) { return UpdateRolesResult.Failure; }
            DataLayer.Models.DiscordRole newRole = new DataLayer.Models.DiscordRole
            {
                DiscordRoleId = discordRole,
                ActivityId = activity.Id,
                Threshold = threshold
            };
            db.DiscordRoles.Add(newRole);
            await db.SaveChangesAsync();
            return UpdateRolesResult.Success;
        }

        public async Task<UpdateRolesResult> ChangeRole(int bossId, int threshold = -1, string discordRole = "")
        {
            DataLayer.Models.DiscordRole existingRole = await db.DiscordRoles.Where(x => x.Activity.OsrsId == bossId).FirstOrDefaultAsync();
            if (existingRole == null) { return UpdateRolesResult.Failure; }
            if (threshold != -1) { existingRole.Threshold = threshold; }
            if (discordRole != "") { existingRole.DiscordRoleId = discordRole; }
            await db.SaveChangesAsync();
            return UpdateRolesResult.Success;
        }
    }
}
