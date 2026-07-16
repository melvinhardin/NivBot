using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class SkillEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int HiscoreIndex { get; set; }
    }
}
