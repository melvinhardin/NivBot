using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.Features.LinkOsrsAccount
{
    public enum LinkAccountResult
    {
        FailureUserNotRegistered,
        FailureNotOnHighscores,
        FailureOsrsNameAlreadyTaken,
        FailureDatabaseSaveFailed,
        SuccessAccountAdded
        
    }
}
