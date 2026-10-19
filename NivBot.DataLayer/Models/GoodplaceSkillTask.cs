using NivBot.DataLayer.Enums;
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

        // Summed XP of this skill over ALL of the users runescape accounts when the task was assigned (the baseline)
        public long SummedCurrentXp { get; set; }
        // Summed XP the user has to reach to complete the task (baseline + XP to gain)
        public long GoalXp { get; set; }

        // UTC time of the last skip, used for the skip cooldown
        public DateTime? LastSkippedAt { get; set; }
    }
}
