using NivBot.DataLayer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class SkillTaskBlocklist
    {
        public int GoodplaceUserId { get; set; }
        public GoodplaceUser GoodplaceUser { get; set; }
        public Skills Skill { get; set; }
    }
}
