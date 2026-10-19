using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class GoodplaceActivityTask
    {
        public int GoodplaceUserId { get; set; }
        public GoodplaceUser GoodplaceUser { get; set; }
        public int ActivityId { get; set; }
        public Activity Activity { get; set; }

        // Summed kill count over ALL of the users runescape accounts when the task was assigned (the baseline)
        public int CurrentCumulativeAmount { get; set; }
        // Summed kill count the user has to reach to complete the task (baseline + kills to get)
        public int GoalAmount { get; set; }

        // UTC time of the last skip, used for the skip cooldown
        public DateTime? LastSkippedAt { get; set; }
    }
}
