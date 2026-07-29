using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.Features.RegisterGoodplaceUser
{
    public enum RegisterGoodplaceUserResult
    {
        Failure,
        FailureUserAlreadyExists,
        FailureSavingToDb,
        Success
    }
}
