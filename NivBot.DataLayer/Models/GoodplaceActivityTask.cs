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

        // This is the cumulative amount of the activity done by ALL of the users runescape accounts
        public int CurrentCumulativeAmount { get; set; }
        public int GoalAmount { get; set; }
    }
}
