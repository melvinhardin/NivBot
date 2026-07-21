using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class ActivityRank
    {
        public int Id { get; set; }
        
        public int ActivityId { get; set; }
        public Activity Activity { get; set; }
        public long? DiscordRoleId { get; set; }
        public DiscordRole DiscordRole { get; set; }
        public int Amount { get; set; }

    }
}
