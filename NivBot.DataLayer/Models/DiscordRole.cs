using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class DiscordRole
    {
        public long DiscordRoleId { get; set; }
        public int ActivityId { get; set; }
        public Activity Activity { get; set; }
        public int Threshold { get; set; }
        
    }
}
