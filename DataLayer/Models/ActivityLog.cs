using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class ActivityLog
    {
        public int Amount { get; set; }

        public int ActivityId { get; set; }
        public Activity Activity { get; set; }

        public string RunescapeAccountId { get; set; }
        public RunescapeAccount RunescapeAccount { get; set; }
    }
}
