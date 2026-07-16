using NivBot.DataLayer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class RunescapeStat
    {
        public int RunescapeAccountId { get; set; }
        public RunescapeAccount RunescapeAccount { get; set; }

        public Skills Skill { get; set; }
        public int Xp { get; set; }
    }
}
