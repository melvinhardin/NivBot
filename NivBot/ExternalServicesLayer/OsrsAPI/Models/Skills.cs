using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.ExternalServicesLayer.OsrsAPI.Models
{
    public class Skills
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required int Rank { get; set; }
        public required int Level { get; set; }
        public required long Xp { get; set; }
    }
}
