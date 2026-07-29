using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.Features.SyncActivities
{
    public enum SyncActivitiesResult
    {
        Success,
        Failure,
        FailureOsrsApiConnection,
        FailureDbConnection,
        FailureNameChange,
        FailureDbSave,
        SuccessNoChanges
    }
}
