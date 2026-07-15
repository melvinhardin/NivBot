using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    internal class ActivityLog
    {
        public int Ammount { get; set; }

        public int ActivityId { get; set; }
        public Activity Activity { get; set; }

        public string RunescapeAccountName { get; set; }
        public RunescapeAccount RunescapeAccount { get; set; }
    }
}
