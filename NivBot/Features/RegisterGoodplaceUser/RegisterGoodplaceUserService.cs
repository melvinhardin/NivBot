using Microsoft.EntityFrameworkCore;
using NivBot.DataLayer;
using NivBot.DataLayer.Models;
using NivBot.Features.LinkOsrsAccount;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.Features.RegisterGoodplaceUser
{
    public class RegisterGoodplaceUserService(GoodplaceContext db)
    {
        public async Task<RegisterGoodplaceUserResult> RegisterGoodplaceUser(long discId)
        {
            // Failurestates

            // See if the user already exists

            GoodplaceUser? user = await db.GoodplaceUsers.FirstOrDefaultAsync(x => x.DiscordUserId == discId);
            if (user != null) { return RegisterGoodplaceUserResult.FailureUserAlreadyExists; }


            // Set up a new goodplace user with an empty wallet.

            GoodplaceUser newUser = new GoodplaceUser
            {
                DiscordUserId = discId,
                Wallet = new Wallet { }
            };

            try
            {
                db.Add(newUser);
                await db.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                return RegisterGoodplaceUserResult.FailureSavingToDb;
            }

            return RegisterGoodplaceUserResult.Success;
        }
    }
}
