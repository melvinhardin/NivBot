using System;
using System.Collections.Generic;
using System.Text;
using NivBot.DataLayer.Enums;

namespace NivBot.DataLayer.Models
{
    public class SkillRank
    {
        public int Id { get; set; }
        public Skills Skill { get; set; }
        public long? DiscordRoleId { get; set; }
        public DiscordRole DiscordRole { get; set; }
        public int Amount { get; set; }
    }
}
