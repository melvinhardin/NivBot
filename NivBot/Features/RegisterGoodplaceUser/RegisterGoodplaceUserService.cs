using NivBot.DataLayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.Features.RegisterGoodplaceUser
{
    public class RegisterGoodplaceUserService(GoodplaceContext db)
    {
        public async Task<RegisterGoodplaceUserResult> RegisterGoodplaceUser()
        {
            return RegisterGoodplaceUserResult.Failure;
        }
    }
}
