using NivBot.DataLayer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class SkillCompetition
    {
        public int Id { get; set; }
        public CompetitionProviderDetails CompetitionProviderDetails { get; set; }
        public Skills Skill{ get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
