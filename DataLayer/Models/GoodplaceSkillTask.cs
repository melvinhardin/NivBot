using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class GoodplaceSkillTask
    {
        public int GoodplaceUserId { get; set; }
        public GoodplaceUser GoodplaceUser { get; set; }

        public Skills Skill { get; set; }

        // This is the cumulative amount of the activity done by ALL of the users runescape accounts
        public long SummedCurrentXp { get; set; }
        public long GoalXp { get; set; }
    }
}
