using System;
using System.Collections.Generic;
using System.Text;
using NivBot.DataLayer.Enums;
namespace NivBot.DataLayer.Models
{
    public class Skill
    {
        public Skills Id { get; set; }
        public required string Name { get; set; }
        public int HiscoreIndex { get; set; }
    }
}
